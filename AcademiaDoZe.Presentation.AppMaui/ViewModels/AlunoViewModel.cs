// Alifer Granemann

using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

[QueryProperty(nameof(AlunoId), "Id")]
public partial class AlunoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly ILogradouroService _logradouroService;

    private int _alunoId;
    private bool _isEditMode;
    private string _cepBusca = string.Empty;
    private LogradouroDto? _logradouroSelecionado;

    private AlunoDto _aluno = CriarAlunoVazio();

    public AlunoDto Aluno
    {
        get => _aluno;
        set => SetProperty(ref _aluno, value);
    }

    public int AlunoId
    {
        get => _alunoId;
        set => SetProperty(ref _alunoId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string CepBusca
    {
        get => _cepBusca;
        set => SetProperty(ref _cepBusca, value);
    }

    public ObservableCollection<LogradouroDto> Logradouros { get; } = [];

    public LogradouroDto? LogradouroSelecionado
    {
        get => _logradouroSelecionado;
        set
        {
            if (SetProperty(ref _logradouroSelecionado, value))
            {
                Aluno.Endereco = value;

                if (value is not null)
                    CepBusca = value.Cep;
            }
        }
    }

    public AlunoViewModel(
        IAlunoService alunoService,
        ILogradouroService logradouroService)
    {
        _alunoService = alunoService;
        _logradouroService = logradouroService;

        Title = "Cadastro de Aluno";
    }

    private static AlunoDto CriarAlunoVazio()
    {
        return new AlunoDto
        {
            Nome = string.Empty,
            Cpf = string.Empty,
            DataNascimento = DateOnly.FromDateTime(DateTime.Today),
            Telefone = string.Empty,
            Email = string.Empty,
            Numero = string.Empty,
            Complemento = string.Empty,
            Senha = string.Empty
        };
    }

    public async Task InitializeAsync()
    {
        await CarregarLogradourosAsync();

        if (AlunoId > 0)
        {
            IsEditMode = true;
            Title = "Editar Aluno";
            await CarregarAlunoAsync();
        }
        else
        {
            IsEditMode = false;
            Title = "Novo Aluno";
            Aluno = CriarAlunoVazio();
            LogradouroSelecionado = null;
            CepBusca = string.Empty;
        }
    }

    private async Task CarregarLogradourosAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var resultado = await _logradouroService.ObterTodosAsync(
                cts.Token);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Logradouros.Clear();

                foreach (var logradouro in resultado)
                    Logradouros.Add(logradouro);
            });
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível carregar os logradouros: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BuscarCpfAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(Aluno.Cpf))
        {
            await ExibirAlertaAsync("CPF", "Digite o CPF para pesquisar.");
            return;
        }

        try
        {
            IsBusy = true;

            var resultado = await _alunoService.ObterPorCpfAsync(
                Aluno.Cpf.Trim());

            if (resultado is null)
            {
                await ExibirAlertaAsync(
                    "Não encontrado",
                    "Não existe aluno cadastrado com esse CPF.");

                return;
            }

            Aluno = resultado;
            AlunoId = resultado.Id;
            IsEditMode = true;
            Title = "Editar Aluno";

            LogradouroSelecionado = resultado.Endereco is null
                ? null
                : Logradouros.FirstOrDefault(
                    l => l.Id == resultado.Endereco.Id);

            CepBusca = resultado.Endereco?.Cep ?? string.Empty;
        }
        catch (ArgumentException ex)
        {
            await ExibirAlertaAsync("CPF inválido", ex.Message);
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível pesquisar o CPF: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarFotoAsync()
    {
        try
        {
            var arquivo = await FilePicker.Default.PickAsync(
                new PickOptions
                {
                    PickerTitle = "Selecione a foto do aluno",
                    FileTypes = FilePickerFileType.Images
                });

            if (arquivo is null)
                return;

            await using var stream = await arquivo.OpenReadAsync();
            using var memoria = new MemoryStream();

            await stream.CopyToAsync(memoria);

            Aluno.Foto = new ArquivoDto
            {
                Conteudo = memoria.ToArray()
            };

            OnPropertyChanged(nameof(Aluno));
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível selecionar a imagem: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BuscarCepAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(CepBusca))
        {
            await ExibirAlertaAsync("CEP", "Digite um CEP para pesquisar.");
            return;
        }

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(10));

            var resultado = await _logradouroService.ObterPorCepAsync(
                CepBusca.Trim(),
                cts.Token);

            if (resultado is null)
            {
                await ExibirAlertaAsync(
                    "CEP não encontrado",
                    "Esse CEP não está cadastrado. Cadastre o logradouro primeiro.");

                return;
            }

            var existente = Logradouros.FirstOrDefault(
                l => l.Id == resultado.Id);

            if (existente is null)
            {
                Logradouros.Add(resultado);
                existente = resultado;
            }

            LogradouroSelecionado = existente;
        }
        catch (OperationCanceledException)
        {
            await ExibirAlertaAsync(
                "Tempo esgotado",
                "A consulta do CEP demorou demais. Tente novamente.");
        }
        catch (ArgumentException ex)
        {
            await ExibirAlertaAsync("CEP inválido", ex.Message);
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível consultar o CEP: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadAlunoAsync()
    {
        await CarregarAlunoAsync();
    }

    private async Task CarregarAlunoAsync()
    {
        if (AlunoId <= 0)
            return;

        try
        {
            IsBusy = true;

            var resultado = await _alunoService.ObterPorIdAsync(AlunoId);

            if (resultado is null)
            {
                await ExibirAlertaAsync("Aviso", "Aluno não encontrado.");
                return;
            }

            Aluno = resultado;

            LogradouroSelecionado = resultado.Endereco is null
                ? null
                : Logradouros.FirstOrDefault(
                    l => l.Id == resultado.Endereco.Id);

            CepBusca = resultado.Endereco?.Cep ?? string.Empty;
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível carregar o aluno: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAlunoAsync()
    {
        if (IsBusy)
            return;

        if (!await ValidarAlunoAsync())
            return;

        try
        {
            IsBusy = true;

            Aluno.Nome = Aluno.Nome.Trim();
            Aluno.Cpf = Aluno.Cpf.Trim();
            Aluno.Telefone = Aluno.Telefone.Trim();

            Aluno.Email = string.IsNullOrWhiteSpace(Aluno.Email)
                ? null
                : Aluno.Email.Trim();

            Aluno.Numero = Aluno.Numero.Trim();

            Aluno.Complemento = string.IsNullOrWhiteSpace(Aluno.Complemento)
                ? null
                : Aluno.Complemento.Trim();

            Aluno.Endereco = LogradouroSelecionado;

            if (IsEditMode)
            {
                await _alunoService.AtualizarAsync(Aluno);

                await ExibirAlertaAsync(
                    "Sucesso",
                    "Aluno atualizado com sucesso!");
            }
            else
            {
                await _alunoService.AdicionarAsync(Aluno);

                await ExibirAlertaAsync(
                    "Sucesso",
                    "Aluno cadastrado com sucesso!");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await ExibirAlertaAsync("Regra de negócio", ex.Message);
        }
        catch (ArgumentException ex)
        {
            await ExibirAlertaAsync("Validação", ex.Message);
        }
        catch (Exception ex)
        {
            await ExibirAlertaAsync(
                "Erro",
                $"Não foi possível salvar o aluno: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task<bool> ValidarAlunoAsync()
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(Aluno.Nome))
            erros.Add("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(Aluno.Cpf))
            erros.Add("CPF é obrigatório.");

        if (string.IsNullOrWhiteSpace(Aluno.Telefone))
            erros.Add("Telefone é obrigatório.");

        if (string.IsNullOrWhiteSpace(Aluno.Numero))
            erros.Add("Número do endereço é obrigatório.");

        if (LogradouroSelecionado is null)
            erros.Add("Busque um CEP cadastrado e selecione o logradouro.");

        if (Aluno.DataNascimento > DateOnly.FromDateTime(DateTime.Today))
            erros.Add("A data de nascimento não pode estar no futuro.");

        if (erros.Count == 0)
            return true;

        await ExibirAlertaAsync(
            "Verifique os campos",
            string.Join("\n", erros));

        return false;
    }

    private static async Task ExibirAlertaAsync(
        string titulo,
        string mensagem)
    {
        if (Shell.Current is not null)
            await Shell.Current.DisplayAlertAsync(titulo, mensagem, "OK");
    }
}