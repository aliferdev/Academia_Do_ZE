// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Application.Mappings;

public static class LogradouroMappingExtensions
{
    public static LogradouroDto ToDto(this Logradouro entity)
    {
        return new LogradouroDto
        {
            Id = entity.Id,
            Cep = entity.Cep.Valor,
            Nome = entity.Nome,
            Bairro = entity.Bairro,
            Cidade = entity.Cidade,
            Estado = entity.Estado,
            Pais = entity.Pais
        };
    }

    public static Logradouro ToEntity(this LogradouroDto dto)
    {
        var result = Logradouro.Criar(
            dto.Id,
            dto.Cep,
            dto.Nome,
            dto.Bairro,
            dto.Cidade,
            dto.Estado,
            dto.Pais
        );

        if (result.IsFailure)
            throw new InvalidOperationException(
                string.Join("; ", result.Notifications.Select(n => n.Mensagem))
            );

        return result.Value!;
    }

    public static Logradouro UpdateFromDto(
        this Logradouro logradouro,
        LogradouroDto logradouroDto)
    {
        ArgumentNullException.ThrowIfNull(logradouro);
        ArgumentNullException.ThrowIfNull(logradouroDto);

        var result = Logradouro.Criar(
            logradouro.Id,
            logradouroDto.Cep ?? logradouro.Cep.Valor,
            logradouroDto.Nome ?? logradouro.Nome,
            logradouroDto.Bairro ?? logradouro.Bairro,
            logradouroDto.Cidade ?? logradouro.Cidade,
            logradouroDto.Estado ?? logradouro.Estado,
            logradouroDto.Pais ?? logradouro.Pais
        );

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Erro de validação ao atualizar Logradouro: " +
                $"{string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }

        return result.Value!;
    }
}