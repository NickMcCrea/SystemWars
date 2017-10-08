using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using NickLib;


namespace NickLib.Pathfinding
{
    public class AStar
    {
        public static string LastPathTime { get; set; }
        private const int MaxNumberOfPathfindsPerFrame = 5;
        private Dictionary<NavigationNode, OpenNodeInformation> m_openNodeList;
        private List<NavigationNode> m_closedNodeList;
        private List<NavigationNode> m_pathNodeList;      
        private DateTime m_start;
        private TimeSpan m_timeTaken;
        private List<NavigationNode> m_exclusionList;
        private class OpenNodeInformation
        {
            private int m_startCost;
            private float m_estimatedDestinationCost;
            private NavigationNode m_parent;


            public int StartCost
            {
                get { return m_startCost; }
                set { m_startCost = value; }
            }
            public float EstimatedDestinationCost
            {
                get { return m_estimatedDestinationCost; }
                set { m_estimatedDestinationCost = value; }
            }
            public NavigationNode Parent
            {
                get { return m_parent; }
                set { m_parent = value; }
            }

            public OpenNodeInformation(NavigationNode _node, NavigationNode _parent, int _parentStartCost)
            {
                if (null != _parent)
                {
                    Vector3 distToParentVector = _node.WorldPosition - _parent.WorldPosition;
                    float dist = distToParentVector.Length();
                    m_startCost = _parentStartCost + (int)dist;


                }
                m_parent = _parent;

            }
        }
        private class AsyncPathfindStore
        {
            public NavigationNode Start;
            public NavigationNode End;
            public bool PermitCrossing;
            public PathResultDelegate Callback;
            public AsyncPathfindStore(NavigationNode start, NavigationNode end, bool permitChasmCrossing, PathResultDelegate callBack)
            {
                Start = start;
                End = end;
                PermitCrossing = permitChasmCrossing;
                Callback = callBack;
            }
        }
        private List<AsyncPathfindStore> outstandingRequests = new List<AsyncPathfindStore>();

        public AStar()
        {
            m_openNodeList = new Dictionary<NavigationNode, OpenNodeInformation>();
            m_closedNodeList = new List<NavigationNode>();

        }

        /// <summary>
        /// Return a path that uses the points in the list.
        /// </summary>
        /// <param name="pointsToPassThrough"></param>
        /// <param name="permitChasmCrossings"></param>
        /// <returns></returns>
        public List<NavigationNode> FindPathUsingPoints(List<NavigationNode> pointsToPassThrough)
        {

            List<NavigationNode> finalPath = new List<NavigationNode>();
            bool success;
            for (int i = 0; i < pointsToPassThrough.Count - 2; i++)
            {
                
                var path = FindPath(pointsToPassThrough[i], pointsToPassThrough[i + 1], out success);

                //this will be a duplicate.
                path.RemoveAt(path.Count - 1);

                finalPath.AddRange(path);
            }

           
            var pathLast = FindPath(pointsToPassThrough[pointsToPassThrough.Count - 2], pointsToPassThrough[pointsToPassThrough.Count - 1],  out success);
            finalPath.AddRange(pathLast);

            return finalPath;

        }

        /// <summary>
        /// Returns a path that connects the two blocks.
        /// </summary>
        /// <param name="_startNode"></param>
        /// <param name="_endNode"></param>
        /// <returns></returns>
        public List<NavigationNode> FindPath(NavigationNode _startNode, NavigationNode _endNode, out bool success)
        {
            if (_startNode == _endNode)
            {
                success = true;
                return new List<NavigationNode> { _startNode };
            }

            m_start = DateTime.Now;



            //clear everything.
            m_openNodeList.Clear();
            m_closedNodeList.Clear();
            m_pathNodeList = new List<NavigationNode>();

            //add the start node to the open list.
            AddToOpenList(_startNode, null, 0, _endNode);

            //start processing.
            bool pathFound = false;


            while (!pathFound)
            {
                bool failed;
                if (ProcessNodes(_endNode, out failed, false))
                {
                    pathFound = true;
                }

                if (failed)
                {
                    success = false;
                    return m_pathNodeList;
                }
            }

            RetracePath(_endNode, _startNode);

            m_timeTaken = DateTime.Now - m_start;
            LastPathTime = m_timeTaken.Milliseconds.ToString();


            success = true;
            return m_pathNodeList;
        }

        public delegate void PathResultDelegate(bool success, List<NavigationNode> results);
       
        public void AsynchronousFindPath(NavigationNode startNode, NavigationNode endNode, bool permitChasmCrossing, PathResultDelegate callBack)
        {
            AsyncPathfindStore store = new AsyncPathfindStore(startNode, endNode, permitChasmCrossing, callBack);
            outstandingRequests.Add(store);
            //callBack(FindPath(startNode, endNode, permitChasmCrossing));
        }

        public void Update(GameTime gameTime)
        {
            int requestsServiced = 0;
            var doneList = new List<AsyncPathfindStore>();
            foreach (AsyncPathfindStore request in outstandingRequests)
            {
                bool success;
                List<NavigationNode> results = FindPath(request.Start, request.End, out success);
                request.Callback(success,results);
                requestsServiced++;
                doneList.Add(request);
                if (requestsServiced > MaxNumberOfPathfindsPerFrame)
                    break;

            }

            foreach (AsyncPathfindStore request in doneList)
            {
                outstandingRequests.Remove(request);
            }
            doneList.Clear();

           
        }

        /// <summary>
        /// Returns a path that excludes those on the exclusion list, if available.
        /// </summary>
        /// <param name="_startNode"></param>
        /// <param name="_endNode"></param>
        /// <param name="exclusionList"></param>
        /// <returns></returns>
        public List<NavigationNode> FindPathThatExcludesNodes(NavigationNode _startNode, NavigationNode _endNode, List<NavigationNode> exclusionList)
        {
            m_exclusionList = exclusionList;

            m_start = DateTime.Now;

            //clear everything.
            m_openNodeList.Clear();
            m_closedNodeList.Clear();
            m_pathNodeList = new List<NavigationNode>();

            //add the start node to the open list.
            AddToOpenList(_startNode, null, 0, _endNode);

            //start processing.
            bool pathFound = false;


            while (!pathFound)
            {
                bool failed;
                if (ProcessNodes(_endNode, out failed, true))
                {
                    pathFound = true;
                }

                if (failed) return m_pathNodeList;
            }

            RetracePath(_endNode, _startNode);

            m_timeTaken = DateTime.Now - m_start;
            LastPathTime = m_timeTaken.Milliseconds.ToString();
            return m_pathNodeList;
        }

        #region Private Methods
        private void RetracePath(NavigationNode _endNode, NavigationNode _startNode)
        {
            //at this point, the open node list contains the goal point, and a 
            //way to retrace our steps back to the start.
            bool finished = false;
            NavigationNode currentNode = _endNode;

            while (!finished)
            {
                m_pathNodeList.Add(currentNode);

                currentNode = m_openNodeList[currentNode].Parent;

                if (currentNode == _startNode)
                {
                    m_pathNodeList.Add(_startNode);
                    finished = true;
                }
            }

            m_pathNodeList.Reverse();

        }

        private void AddToOpenList(NavigationNode _node, NavigationNode _parent, int _parentStartCost, NavigationNode _goalNode)
        {
            //first, calculate estimated distance from goal in 'steps'.
            float distance = Vector3.Distance(_node.WorldPosition, _goalNode.WorldPosition);

            //add the node plus it's accompanying info into the dictionary.
            OpenNodeInformation info = new OpenNodeInformation(_node, _parent, _parentStartCost);
            info.EstimatedDestinationCost = distance;
            m_openNodeList.Add(_node, info);

        }

        private bool ProcessNodes(NavigationNode _goalNode, out bool failed, bool useExclusionList)
        {
            //we want to examine the open list, select the best looking candidate
            //and then process that.
            NavigationNode currentBestNode = SelectBestOpenCandidate();
            failed = false;

            //no path possible.
            if (currentBestNode == null)
            {
                failed = true;
                return false;
            }

            //add any new nodes onto the list.
            foreach (NavigationNode node in currentBestNode.Neighbours)
            {

                //if it's valid.
                if (node.Navigable)
                {

                

                    //we wish to exclude certain nodes from consideration
                    if (useExclusionList)
                    {
                        if (m_exclusionList.Contains(node))
                            continue;
                    }

                    //and if it's not in the open list already.
                    if (!m_openNodeList.ContainsKey(node))
                    {
                        //and if it's not in the closed list.
                        if (!m_closedNodeList.Contains(node))
                        {

                            //add it.
                            AddToOpenList(node, currentBestNode, m_openNodeList[currentBestNode].StartCost, _goalNode);

                            if (node == _goalNode)
                            {
                                //we've found a path!
                                return true;
                            }

                        }
                    }
                }
            }

            //we've look at it, so add it to the closed list.
            m_closedNodeList.Add(currentBestNode);
            return false;

        }

        private NavigationNode SelectBestOpenCandidate()
        {
            //iterate through the current open node list.
            NavigationNode bestNode = null;
            int currentBestScore = Int32.MaxValue; //arbitrarily high.
            foreach (NavigationNode node in m_openNodeList.Keys)
            {
                if (m_closedNodeList.Contains(node))
                    continue;
                OpenNodeInformation info = m_openNodeList[node];
                int nodeScore = info.StartCost + (int)info.EstimatedDestinationCost;

                if (nodeScore < currentBestScore)
                {
                    bestNode = node;
                    currentBestScore = nodeScore;
                }
            }

            return bestNode;
        }
        #endregion

    }
}