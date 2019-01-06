using RestSharp;
using System;
using System.Collections.Generic;

namespace MonoGameEngineCore.DoomLib
{


    public class DoomAPIHandler
    {


        private bool awaitRequestReturn = true;
        private static object _listLock = new object();
        private RestClient client;
        private Queue<RestResponse> completedResponses;
        private LinkedList<RestRequest> unsentRequests;
        public List<FrquentRequestHandler> frequentRequests;
        RestRequest playerRequest;
        RestRequest worldRequest;
        Dictionary<string, CoolDownHandler> coolDownTimers;


        public class CoolDownHandler
        {
            public int CoolDown;
            public DateTime LastExecuted;
        }
        public class FrquentRequestHandler
        {
            public DateTime TimeSinceLastRequestPlaced { get; set; }
            public int Fequency { get; set; }
            public RestRequest Request { get; set; }
            public Action<RestResponse> Action { get; set; }

            public FrquentRequestHandler()
            {


            }

        }

        private bool inFlight = false;

        public DoomAPIHandler(string baseUrl, int port)
        {
            client = new RestClient(baseUrl + ":" + port + "/api/");
            completedResponses = new Queue<RestResponse>();
            unsentRequests = new LinkedList<RestRequest>();
            playerRequest = new RestRequest("player");
            worldRequest = new RestRequest("world/objects");
            frequentRequests = new List<FrquentRequestHandler>();
            coolDownTimers = new Dictionary<string, CoolDownHandler>();

        }

        public void EnqueueRequest(bool priority, RestRequest request, Action<RestResponse> responseAction)
        {
            request.UserState = responseAction;

            if (priority)
                unsentRequests.AddFirst(request);
            else
                unsentRequests.AddLast(request);
        }

        public void EnqueueRequest(bool priority, string tag, RestRequest request)
        {
            request.UserState = tag;

            if (priority)
                unsentRequests.AddFirst(request);
            else
                unsentRequests.AddLast(request);


        }

        public void CreateRegularRequest(int frequencyInMilliseconds, RestRequest request, Action<RestResponse> response)
        {
            FrquentRequestHandler rq = new FrquentRequestHandler()
            {
                Request = request,
                Fequency = frequencyInMilliseconds,
                Action = response,
                TimeSinceLastRequestPlaced = DateTime.Now
            };

            frequentRequests.Add(rq);

        }

        public void Update()
        {
            //don't send anything if we have an outstanding request. Restful doom don't like it!
            if (!inFlight || !awaitRequestReturn)
            {
                if (unsentRequests.Count > 0)
                {

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


            foreach (FrquentRequestHandler rq in frequentRequests)
            {
                var timeSinceInvoked = DateTime.Now - rq.TimeSinceLastRequestPlaced;
                if (timeSinceInvoked.TotalMilliseconds > rq.Fequency)
                {
                    rq.TimeSinceLastRequestPlaced = DateTime.Now;
                    EnqueueRequest(false, rq.Request, rq.Action);
                }
            }

            InvokeResponseActions();
        }

        private void InvokeResponseActions()
        {
            if (completedResponses.Count == 0)
                return;

            var request = completedResponses.Peek().Request as RestRequest;

            if (request.UserState is Action<RestResponse>)
            {
                var action = request.UserState as Action<RestResponse>;
                action.Invoke(completedResponses.Dequeue());
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

        internal void EnqueueCoolDownRequest(string type, RestRequest request, int coolDownInMilliseconds, Action<RestResponse> responseAction)
        {
            if (!coolDownTimers.ContainsKey(type))
            {
                coolDownTimers.Add(type, new CoolDownHandler() { LastExecuted = DateTime.Now, CoolDown = coolDownInMilliseconds });
                EnqueueRequest(false, request, responseAction);
            }
            else
            {
                //get the timer
                var coolDownThing = coolDownTimers[type];

                if ((DateTime.Now - coolDownThing.LastExecuted).TotalMilliseconds > coolDownThing.CoolDown)
                {
                    coolDownThing.LastExecuted = DateTime.Now;
                    EnqueueRequest(false, request, responseAction);
                }
            }
        }
    }


    public class PlayerAction
    {
        public string type { get; set; }
        public float amount { get; set; }
    }

    public class PlayerTurnAction
    {
        public string type { get; set; }
        public float target_angle { get; set; }
    }

  

}
