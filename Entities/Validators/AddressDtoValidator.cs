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
    public class AddressDtoValidator: AbstractValidator<AddressDto>
    {
        public AddressDtoValidator() 
        {
            RuleFor(address => address.AddressLine).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Adres alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("Adres en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Adres en fazla 50 karakter olabilir.");

            RuleFor(address => address.AddressLine2)
                .MaximumLength(50).WithMessage("Adres en fazla 50 karakter olabilir.");

            RuleFor(address => address.City).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Şehir alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("Şehir en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Şehir en fazla 20 karakter olabilir.");

            RuleFor(address => address.Country).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Ülke alanı boş geçilemez.")
                .MinimumLength(3).WithMessage("Ülke en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Ülke en fazla 20 karakter olabilir.");
        }
    }
}
