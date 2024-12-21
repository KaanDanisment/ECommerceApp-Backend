using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class UserRegisterDtoValidator: AbstractValidator<UserRegisterDto>
    {
        public UserRegisterDtoValidator() 
        {
            RuleFor(user => user.FirstName)
                .NotEmpty().WithMessage("İsim alanı boş geçilemez.")
                .MaximumLength(20).WithMessage("İsim en fazla 20 karakter olabilir.");

            RuleFor(user => user.LastName)
                .NotEmpty().WithMessage("Soyisim alanı boş geçilemez.")
                .MaximumLength(20).WithMessage("Soyisim en fazla 20 karakter olabilir.");

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage("Email alanı boş olamaz")
                .EmailAddress().WithMessage("Geçersiz email formatı");

            RuleFor(user => user.PhoneNumber)
                .NotEmpty().WithMessage("Telefon numarası boş geçilemez")
                .Matches(@"^\d{10}$").WithMessage("Telefon numarasıen fazla 10 rakam içerebilir");

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage("Şifre alanı boş geçilemez")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakterli olmalı")
                .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
                .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
                .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Şifre en az bir özel karakter içermelidir (Örn. !@#$%^&*(),.?\":{}|<>)")
                .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir");

            RuleFor(user => user.ConfirmedPassword)
                .NotEmpty().WithMessage("Şifre doğrulama alanı boş geçilemez")
                .Equal(user => user.Password).WithMessage("Şifreler aynı değil! Lütfen kontrol edin.");
        }
    }
}
