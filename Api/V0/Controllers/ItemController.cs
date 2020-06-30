﻿using Domain.Tipos;
using Domain.Validadores;
using FluentValidation.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services;
using System.Collections.Generic;

namespace V0.Api.Controllers
{
    /// <summary>
    /// Itens
    /// </summary>
    [ApiController]
    [ApiVersion("0.1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;
        private readonly ItemService _service;
        private readonly ItemValidator _validator;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="service"></param>
        /// <param name="validator"></param>
        public ItemController(ILogger<ItemController> logger,
                                ItemService service,
                                ItemValidator validator)
        {
            _logger = logger;
            _service = service;
            _validator = validator;
        }

        /// <summary>
        /// Obtém um item pela chave
        /// </summary>
        /// <param name="key">chave de identificação do item</param>
        /// <response code="200">Item com a chave especificada</response>
        /// <response code="404">Item não encontrado</response>
        [HttpGet("{key}")]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult<Item> Get(string key)
        {
            _logger.LogTrace("ItemController.Get({key}) : ENTRYPOINT", key);
            var result = _service.ObterPorId(key);
            _logger.LogTrace("ItemController.Get({key}) : EXITPOINT - SUCCESS", result);
            return result != null ? (ActionResult<Item>) Ok(result) : NotFound();
        }

        /// <summary>
        /// Obtém todos os itens cadastrados
        /// </summary>
        /// <response code="200">Todos os itens cadastrados</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Item>), StatusCodes.Status200OK)]
        public IEnumerable<Item> GetAll()
        {
            _logger.LogTrace("ItemController.GetAll() : ENTRYPOINT");
            var result = _service.ObterTodos();
            _logger.LogTrace("ItemController.GetAll() : EXITPOINT - SUCCESS");
            return result;
        }

        /// <summary>
        /// Insere um novo item
        /// </summary>
        /// <param name="item"></param>
        /// <response code="201">Item cadastrado</response>
        /// <response code="400">Lista de erros de validação</response>
        [HttpPost]
        [ProducesResponseType(typeof(Item), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public CreatedAtActionResult Post([Validator(typeof(PostItemValidator))] Item item)
        {
            _logger.LogTrace("ItemController.Post({dto}) : ENTRYPOINT", item);
            _service.Inserir(item);
            var result = new CreatedAtActionResult(nameof(Get), "Item", new { version = "0.1", key = item.Key }, item);
            _logger.LogTrace("ItemController.Post({result}) : EXITPOINT - SUCCESS", item);
            return result;
        }

        /// <summary>
        /// Remove um item
        /// </summary>
        /// <param name="key">chave do item para ser removido</param>
        /// <response code="204">Item removido com sucesso</response>
        /// <response code="404">Item não encontrado</response>
        [HttpDelete("{key}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult Delete(string key)
        {
            _logger.LogTrace("ItemController.Delete({key}) : ENTRYPOINT", key);

            var result = _service.Remover(key) ? (ActionResult) NoContent() 
                                               : NotFound();

            _logger.LogTrace("ItemController.Delete({key}) : EXITPOINT - SUCCESS", key);
            return result;
        }

        /// <summary>
        /// Atualiza um item
        /// </summary>
        /// <param name="key">chave do item para ser alterado</param>
        /// <param name="patches">alterações</param>
        /// <response code="200">Item alterado com sucesso</response>
        /// <response code="400">Erros de validação</response>
        /// <response code="404">Item não encontrado</response>
        [HttpPatch("{key}")]
        [ProducesResponseType(typeof(Item), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public ActionResult Patch(string key, [FromBody] JsonPatchDocument<Item> patches)
        {
            _logger.LogTrace("ItemController.Patch({key}) : ENTRYPOINT", key);
            var item = _service.ObterPorId(key);

            if (item == null)
                return NotFound();

            patches.ApplyTo(item);

            var validation = _validator.Validate(item);

            if (!validation.IsValid)
                return BadRequest(validation);

            _service.Alterar(item);

            _logger.LogTrace("ItemController.Patch({key}) : EXITPOINT - SUCCESS", key);
            return Ok(item);
        }
    }
}