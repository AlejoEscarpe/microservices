using CatalogMicroservice.Model;
using CatalogMicroservice.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogMicroservice.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CatalogController(ICatalogRepository catalogRepository) : ControllerBase
{
    // GET: api/<CatalogController>
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var catalogItems = catalogRepository.GetCatalogItems();
        return Ok(catalogItems);
    }

    // GET api/<CatalogController>/653e4410614d711b7fc953a7
    [HttpGet("{id}")]
    [Authorize]
    public IActionResult Get(string id)
    {
        // Validate the id is a valid ObjectId
        if (!MongoDB.Bson.ObjectId.TryParse(id, out _))
        {
            return BadRequest("Invalid id format.");
        }

        var catalogItem = catalogRepository.GetCatalogItem(id);
        if (catalogItem == null)
        {
            return NotFound();
        }

        return Ok(catalogItem);
    }

    // POST api/<CatalogController>
    [HttpPost]
    [Authorize]
    public IActionResult Post([FromBody] CatalogItem catalogItem)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Additional basic validations
        if (string.IsNullOrWhiteSpace(catalogItem.Name) || (catalogItem.Name?.Length ?? 0) > 200)
        {
            ModelState.AddModelError(nameof(catalogItem.Name), "Name is required and must not exceed 200 characters.");
            return BadRequest(ModelState);
        }

        if (catalogItem.Price < 0)
        {
            ModelState.AddModelError(nameof(catalogItem.Price), "Price must be a non-negative value.");
            return BadRequest(ModelState);
        }

        catalogRepository.InsertCatalogItem(catalogItem);
        return CreatedAtAction(nameof(Get), new { id = catalogItem.Id }, catalogItem);
    }

    // PUT api/<CatalogController>
    [HttpPut]
    [Authorize]
    public IActionResult Put([FromBody] CatalogItem? catalogItem)
    {
        if (catalogItem != null)
        {
            catalogRepository.UpdateCatalogItem(catalogItem);
            return Ok();
        }
        return new NoContentResult();
    }

    // DELETE api/<CatalogController>/653e4410614d711b7fc953a7
    [HttpDelete("{id}")]
    [Authorize]
    public IActionResult Delete(string id)
    {
        catalogRepository.DeleteCatalogItem(id);
        return Ok();
    }
}