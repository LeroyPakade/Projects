using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Experimental.System.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public static List<string> UserRequest = new List<string>();

        [HttpPost("UserSignup")]
        public void UserSignup([FromBody]User payLoad)
        {
            try
            {
                MessageQueue mq = GetQueueReference();
                mq.Formatter = new XmlMessageFormatter(new[] { typeof(User) });

                // Add an event handler for the PeekCompleted event.
                mq.PeekCompleted += OnPeekCompleted;

                // Begin the asynchronous peek operation.
                mq.BeginPeek();

                SendOneMessage(payLoad);

                Console.WriteLine("Listening on queue");

                Console.ReadLine();

                Console.WriteLine("Closing listener...");

                // Remove the event handler before closing the queue
                mq.PeekCompleted -= OnPeekCompleted;
                mq.Close();
                mq.Dispose();
            }
            catch(Exception ex )
            {

                throw ex;
            }
        }

        [HttpGet("UserSignupResponse")]
        public List<string> UserSignupResponse()
        {
            return UserRequest;
          
        }

        private static MessageQueue GetQueueReference()
        {
            const string queueName = @".\private$\testqueue";
            MessageQueue queue;
            if (!MessageQueue.Exists(queueName))
                queue = MessageQueue.Create(queueName, true);
            else
                queue = new MessageQueue(queueName);
            return queue;
        }

        async void SendOneMessage(User payLoad)
        {
            int _messageNum = 0;
            // Create a transaction because we are using a transactional queue.
            using (var trn = new MessageQueueTransaction())
            {
                try
                {
                    // Create queue object
                    using (var queue = GetQueueReference())
                    {
                        queue.Formatter = new XmlMessageFormatter();

                        // push message onto queue (inside of a transaction)
                        trn.Begin();
                        _messageNum++; // increment the message number
                        queue.Send(payLoad, payLoad.UserName, trn);
                        trn.Commit();

                        Console.WriteLine("Message {0} queued", _messageNum);
                    }
                }
                catch
                {
                    trn.Abort(); // rollback the transaction
                }
            }
        }
        private static void OnPeekCompleted(Object source, PeekCompletedEventArgs asyncResult)
        {
            // Connect to the queue.
            MessageQueue mq = (MessageQueue)source;

            // create transaction
            using (var txn = new MessageQueueTransaction())
            {
                try
                {
                    // retrieve message and process
                    txn.Begin();
                  
                    var message = mq.Receive(txn);

                    if (message != null)
                    {
                        UserRequest.Add(message.Label);
                    }

                    // message will be removed on txn.Commit.
                    txn.Commit();
                }
                catch (Exception ex)
                {
                    // on error don't remove message from queue
                    Console.WriteLine(ex.ToString());
                    txn.Abort();
                }
            }

            // Restart the asynchronous peek operation.
           // mq.BeginPeek();
        }


    }
}
