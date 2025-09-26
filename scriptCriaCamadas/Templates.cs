namespace scriptCriaCamadas;

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
            public {{NOME}}Repository(I{{CONTEXT}} ctx) : base(ctx)
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

    public const string ValidationTemplate = """
        using Autoware.Template.Business;
        using Autoware.Template.Model.Business;
        using Portal.Autoware.Model.Business;
        namespace Portal.Autoware.Business;

        public class {{NOME}}Validation : AbstractValidation<{{NOME}}ModelView>, I{{NOME}}Validation
        {
        	public {{NOME}}Validation(IGeneralDataRequest generalDataRequest) : base(generalDataRequest)
        	{
        		GeneralValid();
        	}

        	public void Dispose()
        	{
        	}

        	private void GeneralValid()
        	{
        	}
        }
        """;

    public const string IValidationTemplate = """
        using Autoware.Template.Model.Business;
        using FluentValidation;
        namespace Portal.Autoware.Model.Business;

        public interface I{{NOME}}Validation : IValidator, IValidation
        {

        }
        """;

    public const string ModelViewTemplate = """
     using System;

     namespace Portal.Autoware.Model.Business;

     public class {{NOME}}ModelView
     {
        public long Id{{NOME}} { get; set; }

        // Adicione outras propriedades conforme necessário
     }
     """;

    public const string ControllerTemplate = """
     using Autoware.Template.Api.Controller;
     using Microsoft.AspNetCore.Authorization;
     using Microsoft.AspNetCore.Http;
     using Microsoft.AspNetCore.Mvc;
     using Portal.Autoware.Business;
     using Portal.Autoware.Model.Business;
     using Portal.Autoware.Model.Repository;
     using System;
     using System.Collections.Generic;
     using System.ComponentModel.DataAnnotations;
     using System.Linq;
     using System.Text.Json;
     using System.Threading.Tasks;

     namespace Portal.Autoware.Api.Areas;

     [Authorize("Bearer")]
     [Route("api/{{NOME}}/[controller]")]
     [ApiController]

     public class {{NOME}}Controller : ApiController
     {
         private readonly I{{NOME}}Business _{{NOMELOWER}}Business;

         public {{NOME}}Controller(I{{NOME}}Business {{NOMELOWER}}Business) : base({{NOMELOWER}}Business)
         {
             _{{NOMELOWER}}Business = {{NOMELOWER}}Business;
         }

         [HttpGet]
         //[ProducesResponseType(200, Type = typeof()]
         public IActionResult Get()
         {
             return Response(null, true);
         }

     }
     
     """;
}
