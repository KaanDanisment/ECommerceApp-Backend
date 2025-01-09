using Entities.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Validators
{
    public class AdminRegisterDtoValidator : AbstractValidator<AdminRegisterDto>
    {
        public AdminRegisterDtoValidator()
        {
            RuleFor(admin => admin.FirstName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("İsim alanı boş geçilemez.")
                .MaximumLength(20).WithMessage("İsim en fazla 20 karakter olabilir.");

            RuleFor(admin => admin.LastName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Soyisim alanı boş geçilemez.")
                .MaximumLength(20).WithMessage("Soyisim en fazla 20 karakter olabilir.");

            RuleFor(admin => admin.Email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email alanı boş olamaz")
                .EmailAddress().WithMessage("Geçersiz email formatı");

            RuleFor(admin => admin.PhoneNumber).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Telefon numarası boş geçilemez")
                .Matches(@"^[1-9]\d{9}$").WithMessage("Telefon numarası başında 0 olmadan 10 rakam içermelidir.");

            RuleFor(admin => admin.Password).Cascade(CascadeMode.Stop)
                .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakterli olmalı")
                .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir")
                .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir")
                .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Şifre en az bir özel karakter içermelidir (Örn. !@#$%^&*(),.?\":{}|<>)")
                .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir");

            RuleFor(admin => admin.ConfirmedPassword).Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("Şifre doğrulama alanı boş geçilemez")
               .Equal(admin => admin.Password).WithMessage("Şifreler aynı değil! Lütfen kontrol edin.");
        }
    }
}
