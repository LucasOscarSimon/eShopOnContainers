﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Ordering.Application
{
    using MediatR;
    using Microsoft.eShopOnContainers.Services.Ordering.API.Application.Commands;
    using Microsoft.eShopOnContainers.Services.Ordering.Infrastructure.Idempotency;
    using Moq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    public class IdentifierCommandHandlerTest
    {
        private readonly Mock<IRequestManager> _requestManager;
        private readonly Mock<IMediator> _mediator;

        public IdentifierCommandHandlerTest()
        {
            _requestManager = new Mock<IRequestManager>();
            _mediator = new Mock<IMediator>();
        }

        [Fact]
        public async Task Handler_sends_command_when_order_no_exists()
        {
            // Arrange
            var fakeGuid = Guid.NewGuid();
            var fakeOrderCmd = new IdentifiedCommand<CreateOrderCommand, bool>(FakeOrderRequest(), fakeGuid);

            _requestManager.Setup(x => x.ExistAsync(It.IsAny<Guid>()))
               .Returns(Task.FromResult(false));

            _mediator.Setup(x => x.SendAsync(It.IsAny<IAsyncRequest<bool>>()))
               .Returns(Task.FromResult(true));

            //Act
            var handler = new IdentifierCommandHandler<CreateOrderCommand, bool>(_mediator.Object, _requestManager.Object);
            var result = await handler.Handle(fakeOrderCmd);

            //Assert
            Assert.True(result);
            _mediator.Verify(x => x.SendAsync(It.IsAny<IAsyncRequest<bool>>()), Times.Once());
        }

        [Fact]
        public async Task Handler_sends_no_command_when_order_already_exists()
        {
            // Arrange
            var fakeGuid = Guid.NewGuid();
            var fakeOrderCmd = new IdentifiedCommand<CreateOrderCommand, bool>(FakeOrderRequest(), fakeGuid);

            _requestManager.Setup(x => x.ExistAsync(It.IsAny<Guid>()))
               .Returns(Task.FromResult(true));

            _mediator.Setup(x => x.SendAsync(It.IsAny<IAsyncRequest<bool>>()))
               .Returns(Task.FromResult(true));

            //Act
            var handler = new IdentifierCommandHandler<CreateOrderCommand, bool>(_mediator.Object, _requestManager.Object);
            var result = await handler.Handle(fakeOrderCmd);

            //Assert
            Assert.False(result);
            _mediator.Verify(x => x.SendAsync(It.IsAny<IAsyncRequest<bool>>()), Times.Never());
        }

        private CreateOrderCommand FakeOrderRequest(Dictionary<string, object> args = null)
        {
            return new CreateOrderCommand(
                null,
                userId: args != null && args.ContainsKey("userId") ? (string)args["userId"] : null,
                city: args != null && args.ContainsKey("city") ? (string)args["city"] : null,
                street: args != null && args.ContainsKey("street") ? (string)args["street"] : null,
                state: args != null && args.ContainsKey("state") ? (string)args["state"] : null,
                country: args != null && args.ContainsKey("country") ? (string)args["country"] : null,
                zipcode: args != null && args.ContainsKey("zipcode") ? (string)args["zipcode"] : null,
                cardNumber: args != null && args.ContainsKey("cardNumber") ? (string)args["cardNumber"] : "1234",
                cardExpiration: args != null && args.ContainsKey("cardExpiration") ? (DateTime)args["cardExpiration"] : DateTime.MinValue,
                cardSecurityNumber: args != null && args.ContainsKey("cardSecurityNumber") ? (string)args["cardSecurityNumber"] : "123",
                cardHolderName: args != null && args.ContainsKey("cardHolderName") ? (string)args["cardHolderName"] : "XXX",
                cardTypeId: args != null && args.ContainsKey("cardTypeId") ? (int)args["cardTypeId"] : 0);
        }
    }
}
