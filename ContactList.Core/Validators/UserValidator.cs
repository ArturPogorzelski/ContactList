using ContactList.Core.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList.Core.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.Email).NotEmpty().EmailAddress();
            RuleFor(user => user.FirstName).NotEmpty().Length(2, 100);
            RuleFor(user => user.LastName).NotEmpty().Length(2, 100);
        }
    }
}
