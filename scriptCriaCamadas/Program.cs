class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Digite o nome da tabela que deseja adicionar (digite corretamente as letras maiúsculas e minúsculas): ");
        var tableName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(tableName))
        {
            Console.WriteLine("Nome da tabela inválido.");
            return;
        }

        const string Repository = "Repository";
        const string Business = "Business";
        string basePath = Directory.GetCurrentDirectory();
        string dependencyInjectionPath = Path.Combine(basePath, "Portal.Autoware.Infra.CrossCutting.IoC", "NativeInjectorBootStrapper.cs");
        string tableNameFormatado = FormatarNomeTabela(tableName);

        Console.WriteLine("Digite o nome do context que deseja adicionar a nova tabela. Caso queira adicionar ao ContextCore, aperte Enter.");
        var inputContext = Console.ReadLine()?.Trim();

        var contextName = string.IsNullOrWhiteSpace(inputContext) ? "ContextCore.cs" : $"{inputContext}.cs";

        var caminhoContexto = Path.Combine(basePath, "Portal.Autoware.DadosBase", "Contexts", contextName);

        if (!File.Exists(caminhoContexto))
        {
            Console.WriteLine("Contexto não encontrado. Digite um contexto válido");
            return;
        }


        Console.WriteLine($"Gerando arquivos para: {tableNameFormatado}...");

        #region Criação e alteração de arquivos
        await AdicionarDbSet(caminhoContexto, tableNameFormatado); // Adiciona DbSet no contexto informado
        await AdicionarInjecaoDependecia(dependencyInjectionPath, tableNameFormatado, Business); //Cria injeção de dependencia para business
        await AdicionarInjecaoDependecia(dependencyInjectionPath, tableNameFormatado, Repository); //Cria injeção de dependencia para repository

        var camadas = ListGeracaoArquivos(tableNameFormatado, Repository, Business);

        foreach (var camada in camadas)
        {
            var path = Path.Combine(basePath, camada.Pasta, tableNameFormatado, camada.NomeArquivo);
            await GerarArquivos(path, camada.Template, tableNameFormatado);
        }
        #endregion 
    }

    static async Task GerarArquivos(string caminho, string template, string tableName)
    {
        var content = template
            .Replace("{{NOME}}", tableName)
            .Replace("{{NOMELOWER}}", ToLowerFirstChar(tableName))
            .Replace("{{NOMEUPPER}}", tableName.ToUpper());

        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);

        await File.WriteAllTextAsync(caminho, content);

        Console.WriteLine($"Caminho {caminho} criado com sucesso!");
    }

    static async Task AdicionarDbSet(string filePath, string tableName)
    {
        var leituraArquivo = await File.ReadAllLinesAsync(filePath);
        var content = leituraArquivo.ToList();

        if (content.Any(x => x.Contains($"DbSet<{tableName}>")))
        {
            Console.WriteLine($"DbSet<{tableName}> já existe no contexto");
            return;
        }

        var index = content.FindLastIndex(l => l.Contains("DbSet<")); // Encontra a primeira ocorrência de DbSet<

        var currentLine = content[index];
        var indentation = new string(currentLine.TakeWhile(char.IsWhiteSpace).ToArray());
        var dbSetLine = $"{indentation}public DbSet<{tableName}> {tableName} {{ get; set; }}"; // Adiciona a mesma identeção da linha 

        if (index != -1)
        {
            content.Insert(index + 1, dbSetLine); // Adiciona a nova linha após a última ocorrência
        }

        await File.WriteAllLinesAsync(filePath, content);

        Console.WriteLine($"Adicionado DbSet<{tableName}> ao contexto em {filePath}\n");
    }

    static async Task AdicionarInjecaoDependecia(string filePath, string tableName, string camada)
    {
        var leituraArquivo = await File.ReadAllLinesAsync(filePath);
        var content = leituraArquivo.ToList();

        var index = content.FindLastIndex(l => l.Contains("services.AddScoped<")); // Encontra a primeira ocorrência de DbSet<

        var currentLine = content[index];
        var indentation = new string(currentLine.TakeWhile(char.IsWhiteSpace).ToArray());
        var newDependencyInjectionLine = $"{indentation}services.AddScoped<I{tableName}{camada}, {tableName}{camada}>();"; // Adiciona a mesma identeção da linha 

        if (index != -1)
        {
            content.Insert(index + 1, newDependencyInjectionLine); // Adiciona a nova linha após a última ocorrência
        }

        await File.WriteAllLinesAsync(filePath, content);

        Console.WriteLine($"Adicionado injeção de dependência para {camada}.\n");
    }

    static List<GeracaoArquivo> ListGeracaoArquivos(string tableNameFormatado, string Repository, string Business)
    {
        var listArquivos = new List<GeracaoArquivo>();

        listArquivos.Add(Map("Portal.Autoware.Entidades", $"{tableNameFormatado}.cs", Templates.EntityTemplate));
        listArquivos.Add(Map("Portal.Autoware.Model.Repository", $"I{tableNameFormatado}{Repository}.cs", Templates.IRepositoryTemplate));
        listArquivos.Add(Map("Portal.Autoware.Data.Repository", $"{tableNameFormatado}{Repository}.cs", Templates.RepositoryTemplate));
        listArquivos.Add(Map("Portal.Autoware.Model.Business",  $"I{tableNameFormatado}{Business}.cs", Templates.IBusinessTemplate));
        listArquivos.Add(Map("Portal.Autoware.Business", $"{tableNameFormatado}{Business}.cs", Templates.BusinessTemplate));

        return listArquivos;
    }

    static GeracaoArquivo Map(string pasta, string nomeArquivo, string template)
    {
        return new GeracaoArquivo
        {
            Pasta = pasta,
            NomeArquivo = nomeArquivo,
            Template = template
        };
    }

    static string FormatarNomeTabela(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return char.ToUpper(texto[0]) + texto[1..];
    }

    static string ToLowerFirstChar(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return char.ToLower(texto[0]) + texto[1..];
    }
}

public static class Templates
{
    public const string EntityTemplate = """
        using System.ComponentModel.DataAnnotations;
        using System.ComponentModel.DataAnnotations.Schema;

        namespace Portal.Autoware.Model.Repository;

        [Table("{{NOMEUPPER}}")]
        public class {{NOME}}
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public long ID_{{NOMEUPPER}} { get; set; }


            // Adicione outras propriedades conforme necessário
        }
        """;

    public const string RepositoryTemplate = """
        using Autoware.Template.Data.Repository;
        using Portal.Autoware.Model.Repository;

        namespace Portal.Autoware.Data.Repository;

        public class {{NOME}}Repository : Repository<{{NOME}}>, I{{NOME}}Repository
        {
            public {{NOME}}Repository(IContextCore ctx) : base(ctx)
            {
            }
        }
        """;

    public const string IRepositoryTemplate = """
        using Autoware.Template.Model.Repository;

        namespace Portal.Autoware.Model.Repository;

        public interface I{{NOME}}Repository : IRepository<{{NOME}}>
        {
        }
        """;

    public const string BusinessTemplate = """
        using Autoware.Template.Business;
        using Autoware.Template.Model.Business;
        using Portal.Autoware.Model.Business;
        using Portal.Autoware.Model.Repository;

        namespace Portal.Autoware.Business;

        public class {{NOME}}Business : AbstractBusiness, I{{NOME}}Business
        {
            private readonly I{{NOME}}Repository _{{NOMELOWER}}Repository;

            public {{NOME}}Business(IGeneralDataRequest generalDataRequest, I{{NOME}}Repository {{NOMELOWER}}Repository) 
                : base(generalDataRequest)
            {
                _{{NOMELOWER}}Repository = {{NOMELOWER}}Repository;
            }
        }
        """;

    public const string IBusinessTemplate = """
        using Autoware.Template.Model.Business;

        namespace Portal.Autoware.Model.Business;

        public interface I{{NOME}}Business : IAbstractBusiness
        {
        }
        """;
}

public class GeracaoArquivo
{
    public string Pasta { get; set; }
    public string NomeArquivo { get; set; }
    public string Template { get; set; }
}
