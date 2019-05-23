using System;
using System.Collections.Generic;
using System.Text;
using Quobject.SocketIoClientDotNet.Client;
using System.Threading;
using _36E_Business___ERP.security;
using Newtonsoft.Json.Linq;
using _36E_Business___ERP.cloudDB;

namespace _36E_Business___ERP.cloudPush
{
    /*
     * 
     * Here we will connect to the Cloud Push Sockets.
     * We will also have the required offline database and tables.
     * This will also allow us to establish connections and retrieve data.
     * We will also interact with 'Waiter' to do the API connections. 
     * 
     */
    public static class PushManager
    {
        // Implementation.
        /*
         PushManager.Init("Push Token",
                "sender", (string message) => {
                    // Do something with the message.
                });
             */

        // Variables.
        private static Dictionary<string, Message> messages;
        private static string Status { get; set; }
        private static StringBuilder StatusTrace;
        private static string pushToken;
        internal static Network NetworkStatus { get; set; }

        // Callback
        public delegate void OnReceived(string message);
        private static Dictionary<string, OnReceived> onMessageReceivers;  // The methods will be set here in a Dictionary, to be called.
        public delegate void OnInternetConnected(Network network);
        private static List<OnInternetConnected> onInternetConnectedListeners;

        // Enums.
        public enum Network
        {
            CONNECTED, DISCONNECTED, RECONNECTING
        }

        // Methods.
        // Here we will initialize the connection to the Cloud Push.
        public static void Init(string userPushToken, string sender, OnReceived onResponse)
        {
            try
            {
                // Here we will initialize the required variables.
                AddStatus("Going to Initialize");
                if (messages == null)
                {
                    messages = new Dictionary<string, Message>();
                }
                if (onMessageReceivers == null)
                {
                    onMessageReceivers = new Dictionary<string, OnReceived>();
                }
                if (onInternetConnectedListeners == null)
                {
                    onInternetConnectedListeners = new List<OnInternetConnected>();
                }
                SetOnMessageRecieved(sender, onResponse);
                pushToken = userPushToken;
                NetworkStatus = Network.DISCONNECTED;

                // Lets Connect to the Cloud Push Server.
                Thread cloudPushThread = new Thread(PushConnect);
                cloudPushThread.Start();
            }
            catch (Exception e)
            {
                // There was an Error.
                AddStatus("Init Err : " + e.Message);
            }
        }

        // Connect to the Cloud Push Server.
        private static void PushConnect()
        {
            try
            {
                // Here we will initialize the Connection to the Cloud Push Socket Server.
                Socket socket = IO.Socket("http://" + C.CLOUD_PUSH_IP_ADDRESS);
                // Here we will log on to the cloud push service using the token.
                // Network has reconnected.
                socket.On(Socket.EVENT_CONNECT, () =>
                {
                    // Send the Push token to the Cloud Push.
                    socket.Emit("logOn", pushToken);
                    NetworkStatus = Network.CONNECTED;
                    try
                    {
                        // Lets notify all the internet connected listeneres that internet has started
                        for (int i = 0; i < onInternetConnectedListeners.Count; i++)
                        {
                            try
                            {
                                // Lets send the notification.
                                OnInternetConnected onInternetConnected = onInternetConnectedListeners[i];
                                onInternetConnected(Network.CONNECTED);
                            }
                            catch (Exception e)
                            {
                                // There was an Error.
                                // Lets remove the listener.
                                onInternetConnectedListeners.RemoveAt(i);
                                i = i - 1;
                            }
                        }
                    } catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                // Network has disconnected.
                socket.On(Socket.EVENT_DISCONNECT, () =>
                {
                    try
                    {
                        NetworkStatus = Network.DISCONNECTED;
                        // Lets notify all the internet connected listeneres that internet has started
                        for (int i = 0; i < onInternetConnectedListeners.Count; i++)
                        {
                            try
                            {
                                // Lets send the notification.
                                OnInternetConnected onInternetConnected = onInternetConnectedListeners[i];
                                onInternetConnected(Network.DISCONNECTED);
                            }
                            catch (Exception e)
                            {
                                // There was an Error.
                                // Lets remove the listener.
                                onInternetConnectedListeners.RemoveAt(i);
                                i = i - 1;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                // Network has reconnecting.
                socket.On(Socket.EVENT_RECONNECTING, () =>
                {
                    try
                    {
                        NetworkStatus = Network.RECONNECTING;
                        // Lets notify all the internet connected listeneres that internet has started
                        for (int i = 0; i < onInternetConnectedListeners.Count; i++)
                        {
                            try
                            {
                                // Lets send the notification.
                                OnInternetConnected onInternetConnected = onInternetConnectedListeners[i];
                                onInternetConnected(Network.RECONNECTING);
                            }
                            catch (Exception e)
                            {
                                // There was an Error.
                                // Lets remove the listener.
                                onInternetConnectedListeners.RemoveAt(i);
                                i = i - 1;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                // Network has error.
                socket.On(Socket.EVENT_CONNECT_ERROR, () =>
                {
                    try
                    {
                        NetworkStatus = Network.DISCONNECTED;
                        // Lets notify all the internet connected listeneres that internet has started
                        for (int i = 0; i < onInternetConnectedListeners.Count; i++)
                        {
                            try
                            {
                                // Lets send the notification.
                                OnInternetConnected onInternetConnected = onInternetConnectedListeners[i];
                                onInternetConnected(Network.DISCONNECTED);
                            }
                            catch (Exception e)
                            {
                                // There was an Error.
                                // Lets remove the listener.
                                onInternetConnectedListeners.RemoveAt(i);
                                i = i - 1;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                // Network has error.
                socket.On(Socket.EVENT_RECONNECT_ERROR, () =>
                {
                    try
                    {
                        NetworkStatus = Network.DISCONNECTED;
                        // Lets notify all the internet connected listeneres that internet has started
                        for (int i = 0; i < onInternetConnectedListeners.Count; i++)
                        {
                            try
                            {
                                // Lets send the notification.
                                OnInternetConnected onInternetConnected = onInternetConnectedListeners[i];
                                onInternetConnected(Network.DISCONNECTED);
                            }
                            catch (Exception e)
                            {
                                // There was an Error.
                                // Lets remove the listener.
                                onInternetConnectedListeners.RemoveAt(i);
                                i = i - 1;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                // Now we wiil set listerners for messages that have been sent.
                socket.On("message", (data) =>
                {
                    try
                    {
                        // Here we will parse the message data, and determine whose message it is.
                        // ANd we will send it to the corresponding listener.
                        // 1. Parse the Message.
                        JObject msgJObj = JObject.Parse(data.ToString());
                        Message msgObj = new Message();

                        // 2. Get the Sender from the Message.
                        msgObj.Sender = msgJObj["sender"].ToString();

                        // 3. Get the Message data. This message has to be converted from byte array to string.
                        msgObj.MessageData = msgJObj["message"].ToString();
                        string[] byteStrings = msgJObj["message"].ToString().Split(',');
                        byte[] msgBytes = new byte[byteStrings.Length];
                        for (int i = 0; i < byteStrings.Length; i++)
                        {
                            char msgCharTemp = Convert.ToChar(short.Parse(byteStrings[i]));
                            msgBytes[i] = Convert.ToByte(msgCharTemp);
                        }
                        msgObj.MessageValue = Encoding.UTF8.GetString(msgBytes, 0, msgBytes.Length);

                        // 4. Store the Message in the Array for Validation of the Message data.
                        messages[msgJObj["id"].ToString()] = msgObj;

                        // 5. Emit the Checksum to the Cloud Push Service with the Checksum of the Message data.
                        // Create the JSON to store the data for the checksum
                        JObject checkSumJObj = new JObject();
                        checkSumJObj["message_id"] = msgJObj["id"].ToString();
                        checkSumJObj["message_checksum"] = Security.HashMD5(msgObj.MessageData);
                        // Emit the Checksum Data to the Cloud Push Service.
                        socket.Emit("checkMessage", checkSumJObj);
                        AddStatus("Emitted Message to be Validated");
                    }
                    catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                //  Here we receive the validation of the checksum and send it to the user.
                socket.On("validateMessage", (data) =>
                {
                    try
                    {
                        // Here we will get the validation of the message that was sent to us.
                        // Lets Check if the message was validated properly.
                        JObject validateJObj = JObject.Parse(data.ToString());
                        // Get the Message ID.
                        string msgID = validateJObj["message_id"].ToString();
                        bool msgValid = bool.Parse(validateJObj["authorise"].ToString());
                        if (msgValid)
                        {
                            // The message checksum was valid.
                            Message msgObj = messages[msgID];
                            // Lets send the message to the user.
                            SendToUser(msgObj.Sender, msgObj.MessageValue);
                            messages.Remove(msgID);
                        }
                        else
                        {
                            AddStatus("Message is Invalid");
                            // The message was not valid.
                            // Lets delete the message from the dictionary.
                            messages.Remove(msgID);
                        }
                    }
                    catch (Exception e)
                    {
                        // There was an Error.
                    }
                });

                // Successfull Connection.
                AddStatus("Init Success");
            }
            catch (Exception e)
            {
                // There was an Error.
                AddStatus("Init Error : " + e.Message);
            }
        }

        // Set the Callback method for the string.
        private static bool SetOnMessageRecieved(string sender, OnReceived onResponse)
        {
            try
            {
                // Set the Listenner here.
                if (onMessageReceivers.ContainsKey(sender))
                {
                    // Sender Response Listener Found.
                    onMessageReceivers[sender] = onResponse;
                }
                else
                {
                    // The Sender has not been received.
                    onMessageReceivers.Add(sender, onResponse);
                }
                return true;
            }
            catch (Exception e)
            {
                // There was an Error.
                return false;
            }
        }

        // Send the User the Data.
        private static void SendToUser(string sender, string message)
        {
            try
            {
                // Here we will choose the Response Listener and Send it to that user.
                if (onMessageReceivers.ContainsKey(sender))
                {
                    // The Receiver Exists.
                    OnReceived onListener = onMessageReceivers[sender];
                    onListener(message);
                }
            }
            catch (Exception e)
            {
                // There was an Error.
            }
        }

        // Set the Callback method for the internet connection.
        internal static bool SetOnNetworkChangedListener(OnInternetConnected onInternetConnected)
        {
            try
            {
                // Here we will set the Listener.
                onInternetConnectedListeners.Add(onInternetConnected);
                return true;
            } catch (Exception e)
            {
                // There was an Error.
                return false;
            }
        }
        
        // Class to store the messages.
        private class Message
        {
            // Variables.
            internal string Sender { get; set; }
            internal string MessageData { get; set; }
            internal string MessageValue { get; set; }
        }

        // Here we set the status and the status trace.
        private static void AddStatus(string status1)
        {
            try
            {
                // Here we append the status to the status trace and status.
                if (StatusTrace == null)
                {
                    StatusTrace = new StringBuilder();
                }
                Status = status1;
                StatusTrace.Append(status1)
                    .Append('\n');
            }
            catch (Exception e)
            {
                // There was an Error.
            }
        }

        // This will retrieve the whole status trace.
        public static string GetStatusTrace()
        {
            try
            {
                return StatusTrace.ToString();
            }
            catch (Exception e)
            {
                return "Status is Null";
            }
        }

        // This will retrieve just the status of this object.
        public static string GetStatus()
        {
            return Status;
        }
    }
}
