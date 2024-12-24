using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class UserLoginDtoValidator: AbstractValidator<UserLoginDto>
    {
        public UserLoginDtoValidator() 
        {
            RuleFor(user => user.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Lütfen mail adresinizi girin.")
                .EmailAddress().WithMessage("Lütfen Geçerli bir mail adresi girin!");

            RuleFor(user => user.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Lütfen şifrenizi girin.");
        }
    }
}
