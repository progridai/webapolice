using System;

namespace WebApolice.SharedKernel.Application.Security;

public interface IUsuarioCorrente
{
    Guid GetUsuarioId();
    string GetUsuarioEmail();
}
