// <copyright file="BasicTests.cs" company="Microbus contributors">
//  Copyright (c) Microbus contributors. All rights reserved.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class BasicTests
    {
        [Fact]
        public void Send()
        {
            // arrange
            var result = default(string);
            var bus = new Microbus();
            bus.Register<string>(message => result = message);

            // act
            bus.Send("hello");

            // assert
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Send2Messages()
        {
            // arrange
            var result = new List<string>();
            var bus = new Microbus();
            bus.Register<string>(message => result.Add(message));

            // act
            bus.Send("foo");
            bus.Send("bar");

            // assert
            Assert.Equal("foo", result[0]);
            Assert.Equal("bar", result[1]);
        }

        [Fact]
        public void Send2Consumers()
        {
            // arrange
            var result1 = default(string);
            var result2 = default(string);
            var bus = new Microbus();
            bus.Register<string>(message => result1 = message);
            bus.Register<string>(message => result2 = message);

            // act
            bus.Send("hello");

            // assert
            Assert.Equal("hello", result1);
            Assert.Equal("hello", result2);
        }

        [Fact]
        public void SendChainedConsumers()
        {
            // arrange
            var result = default(string);
            var bus = new Microbus();
            bus.Register<string>(message => bus.Send(new Message { Value = message }));
            bus.Register<Message>(message => result = message.Value);

            // act
            bus.Send("hello");

            // assert
            Assert.Equal("hello", result);
        }

        [Fact]
        public void SendCyclic()
        {
            // arrange
            var bus = new Microbus();
            bus.Register<string>(message => bus.Send(message));

            // act
            Action action = () => bus.Send("hello");

            // assert
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.Contains("cyclic", ex.Message);
            }
        }

        // bus x2 unique sends
        [Fact]
        public void SendMutipleBus()
        {
            // arrange
            var result1 = default(string);
            var result2 = default(string);
            var bus1 = new Microbus();
            var bus2 = new Microbus();
            bus1.Register<string>(message => result1 = message);
            bus2.Register<string>(message => result2 = message);

            // act
            bus1.Send("foo");
            bus2.Send("bar");

            // assert
            Assert.Equal("foo", result1);
            Assert.Equal("bar", result2);
        }

        // thread safety:
        // all operations occur on a single thread - everything is marshalled to a single thread

        // exceptions?
        // exceptions bubble up - no wrapped/inner exception
        // nested exceptions (denormalizer) terminate? contain wrapped/inner exception?
        // that's probably all
        [Fact(Skip = "Nonsense")]
        public void ExceptionsBubbleUp()
        {
            // arrange
            var bus = new Microbus();
            bus.Register<string>(message => { throw new Exception(); });

            // act
            bus.Send("hello");

            // assert
            ////Assert.Equal("foo", result1);
            ////Assert.Equal("bar", result2);
        }

        //// performance?

        private class Message
        {
            public string Value { get; set; }
        }
    }
}
