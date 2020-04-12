using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient.Messages;

namespace RosSharp.RosBridgeClient
{
    public class GlobalPositionSubscriber : Subscriber<Messages.NavSatFix>
    {
        public Messages.NavSatFix position;


        protected override void Start()
        {
           position = new Messages.NavSatFix();
           base.Start();
        }

        protected override void ReceiveMessage(Messages.NavSatFix pos)
        {
            position = pos;
            Debug.Log(position.latitude+", "+ position.longitude);
        }
    }
}
