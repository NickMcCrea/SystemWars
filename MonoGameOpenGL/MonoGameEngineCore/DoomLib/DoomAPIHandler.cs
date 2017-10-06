using MonoGameEngineCore.GameObject;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameEngineCore.DoomLib
{
    
   


    public class DoomAPIHandler
    {
  

        private bool awaitRequestReturn = true;
        private static object _listLock = new object();
        private RestClient client;
        private Queue<RestResponse> completedResponses;
        private LinkedList<RestRequest> unsentRequests;
       
        RestRequest playerRequest;
        RestRequest worldRequest;
     
        private bool inFlight = false;

        public DoomAPIHandler(string baseUrl, int port)
        {
            client = new RestClient(baseUrl + ":" + port + "/api/");
            completedResponses = new Queue<RestResponse>();
            unsentRequests = new LinkedList<RestRequest>();
            playerRequest = new RestRequest("player");
            worldRequest = new RestRequest("world/objects");

        }

        public void EnqueueRequest(bool priority, string tag, RestRequest request)
        {
            request.UserState = tag;

            if (priority)
                unsentRequests.AddFirst(request);
            else
                unsentRequests.AddLast(request);


        }

        public void Update()
        {
            //don't send anything if we have an outstanding request. Restful doom don't like it!
            if (!inFlight || !awaitRequestReturn)
            {
                if (unsentRequests.Count == 0)
                    return;

                //get the first
                RestRequest unsentRequest = unsentRequests.First.Value;
                unsentRequests.RemoveFirst();

                inFlight = true;
                client.ExecuteAsync(unsentRequest, response =>
                {
                    lock (_listLock)
                    {
                        completedResponses.Enqueue(response as RestResponse);
                        inFlight = false;
                    }
                });
            }
        }

        public RestResponse GetNextResponse()
        {
            lock (_listLock)
            {
                if (completedResponses.Count > 0)
                    return completedResponses.Dequeue();
            }

            return null;
        }

        public void RequestPlayerDetails()
        {
            EnqueueRequest(false, "playerDetails", playerRequest);
        }

        public void RequestAllWorldDetails()
        {
            EnqueueRequest(false, "worldObjects", worldRequest);
        }

        public void MovePlayerForward(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "forward", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void MovePlayerBackward(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "backward", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void TurnLeft(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "turn-left", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void TurnRight(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "turn-right", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void StrafeLeft(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "strafe-left", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void StrafeRight(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "strafe-right", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }


        public class PlayerAction
        {
            public string type { get; set; }
            public float amount { get; set; }
        }

    }


    

    public class DoomComponent : IComponent
    {

        public string DoomType { get; set; }
        public double Health { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }
        public bool ForwardHitVector { get; set; }
        public bool LeftHitVector { get; set; }
        public bool RightHightVector { get; set; }
        public float HitVectorSize { get; set; }
        public GameObject.GameObject ParentObject
        {
            get; set;
        }

        public void Initialise()
        {

        }

        public void PostInitialise()
        {

        }
    }

}
