using ContactList.Application.Commands.Contact;
using ContactList.Core.Exceptions;
using ContactList.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Application.Handlers
{
    public class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, Unit>
    {
        private readonly IContactRepository _contactRepository;

        public DeleteContactCommandHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<Unit> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
        {
            var contact = await _contactRepository.GetByIdAsync(request.ContactId);
            if (contact == null)
            {
                throw new NotFoundException("Contact not found");
            }

            await _contactRepository.DeleteAsync(request.ContactId);
            return Unit.Value;
        }
    }
}
