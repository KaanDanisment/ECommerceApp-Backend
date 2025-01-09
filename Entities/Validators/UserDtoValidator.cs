using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class UserDtoValidator: AbstractValidator<UserDto>
    {
        public UserDtoValidator() 
        {
            RuleFor(user => user.FirstName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("İsim alanı boş geçilemez.")
                .MaximumLength(20).WithMessage("İsim en fazla 20 karakter olabilir.");

            RuleFor(user => user.LastName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Soyisim alanı boş geçilemez.")
                .MaximumLength(20).WithMessage("Soyisim en fazla 20 karakter olabilir.");

            RuleFor(user => user.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email alanı boş olamaz")
                .EmailAddress().WithMessage("Geçersiz email formatı");

            RuleFor(user => user.PhoneNumber).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Telefon numarası boş geçilemez")
                .Matches(@"^\d{10}$").WithMessage("Telefon numarasıen fazla 10 rakam içerebilir");
        }
    }
}
