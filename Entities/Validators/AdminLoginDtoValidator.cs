using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class AdminLoginDtoValidator: AbstractValidator<AdminLoginDto>
    {
        public AdminLoginDtoValidator()
        {
            RuleFor(admin => admin.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Lütfen mail adresinizi girin.")
                .EmailAddress().WithMessage("Lütfen Geçerli bir mail adresi girin!");

            RuleFor(admin => admin.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Lütfen şifrenizi girin.");
        }
    }
}
