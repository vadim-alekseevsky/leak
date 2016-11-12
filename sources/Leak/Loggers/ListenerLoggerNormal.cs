﻿using System;

namespace Leak.Loggers
{
    public class ListenerLoggerNormal : ListenerLogger
    {
        public override void Handle(string name, object payload)
        {
            HandleMessage(name, payload);
            base.Handle(name, payload);
        }

        private void HandleMessage(string name, dynamic payload)
        {
            switch (name)
            {
                case "listener-started":
                    Console.WriteLine($"{payload.Local}: started listening on port {payload.Port}");
                    break;
            }
        }
    }
}