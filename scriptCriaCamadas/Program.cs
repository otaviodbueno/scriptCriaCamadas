class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Digite o nome da tabela que deseja adicionar: ");
        var tableName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(tableName))
        {
            Console.WriteLine("Nome da tabela inválido.");
            return;
        }

        Console.WriteLine("Digite o nome do context que deseja adicionar a nova tabela. Caso queira adicionar ao ContextCore, aperte Enter.");            
        var inputContext = Console.ReadLine();

        var contextName = string.IsNullOrWhiteSpace(inputContext) ? "ContextCore.cs" : $"{inputContext}.cs";

        string basePath = Directory.GetCurrentDirectory();

        var caminhoContexto = Path.Combine(basePath, "Portal.Autoware.DadosBase", "Contexts", contextName);

        if (!Directory.Exists(caminhoContexto))
        {
            Console.WriteLine("Contexto não encontrado. Digite um contexto válido");
            return;
        }

        var tableNameFormatado = FormatarNomeTabela(tableName);

        Console.WriteLine($"Gerando arquivos para: {tableNameFormatado}\n");

        await AdicionarDbSet(caminhoContexto, tableNameFormatado);


        // Entidade
        string entityPath = Path.Combine(basePath, "Portal.Autoware.Entidades", $"{tableNameFormatado}.cs");
        await GerarArquivos(entityPath, Templates.EntityTemplate, tableNameFormatado);

        // IRepository
        string IRepositoryPath = Path.Combine(basePath, "Portal.Autoware.Model.Repository", $"I{tableNameFormatado}Repository.cs");
        await GerarArquivos(IRepositoryPath, Templates.IRepositoryTemplate, tableNameFormatado);

        // Repository
        string repositoryPath = Path.Combine(basePath, "Portal.Autoware.Data.Repository", $"{tableNameFormatado}Repository.cs");
        await GerarArquivos(repositoryPath, Templates.RepositoryTemplate, tableNameFormatado);

        // IBusiness
        string IBusinessPath = Path.Combine(basePath, "Portal.Autoware.Model.Business", $"I{tableNameFormatado}Business.cs");
        await GerarArquivos(IBusinessPath, Templates.IBusinessTemplate, tableNameFormatado);

        // Business
        string businessPath = Path.Combine(basePath, "Portal.Autoware.Business", $"{tableNameFormatado}Business.cs");
        await GerarArquivos(businessPath, Templates.BusinessTemplate, tableNameFormatado);
    }

    static async Task GerarArquivos(string caminho, string template, string tableName)
    {
        var content = template
            .Replace("{{NOME}}", tableName)
            .Replace("{{NOMELOWER}}", tableName.ToLower())
            .Replace("{{NOMEUPPER}}", tableName.ToUpper());

        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        await File.WriteAllTextAsync(caminho, content);

        Console.WriteLine($"Caminho {caminho} criado com sucesso!\n");
    }

    static string FormatarNomeTabela(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        return char.ToUpper(texto[0]) + texto[1..];
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

        var index = content.FindIndex(l => l.Contains("DbSet<")); // Encontra a primeira ocorrência de DbSet<

        var currentLine = content[index];
        var indentation = new string(currentLine.TakeWhile(char.IsWhiteSpace).ToArray());
        var dbSetLine = $"{indentation}public DbSet<{tableName}> {tableName} {{ get; set; }}"; // Adiciona a mesma identeção da linha 

        if (index != -1)
        {
            content.Insert(index - 1, dbSetLine);
        }

        await File.WriteAllLinesAsync(filePath, content);

        Console.WriteLine($"Adicionado DbSet<{tableName}> ao contexto em {filePath}");
    }

    static async Task AdicionarInjecaoDependecia(string filePath, string tableName)
    {
        var leituraArquivo = await File.ReadAllLinesAsync(filePath);
        var content = leituraArquivo.ToList();

        var index = content.FindIndex(l => l.Contains("services.AddScoped<")); // Encontra a primeira ocorrência de DbSet<

        var currentLine = content[index];
        var indentation = new string(currentLine.TakeWhile(char.IsWhiteSpace).ToArray());
        var repositoryLine = $"{indentation}services.AddScoped<I{tableName}Repository, {tableName}Repository"; // Adiciona a mesma identeção da linha 
        var businessLine = $"{indentation}services.AddScoped<I{tableName}Business, {tableName}Business"; // Adiciona a mesma identeção da linha

        if (index != -1)
        {
            content.Insert(index - 1, repositoryLine);
        }

        await File.WriteAllLinesAsync(filePath, content);

        Console.WriteLine($"Adicionado injeção de dependência.");
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