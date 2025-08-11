using LevelData.Credentials.DIDForge.Abstractions;
using LevelData.Credentials.DIDForge.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenCredentialPublisher.PublishingService.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenCredentialPublisher.PublishingService.Api.Controllers
{
    [ApiController]
    public class DidWebController : ControllerBase
    {
        readonly IDidDocumentGenerator _generator;
        readonly IIssuerService _issuerService;

        public DidWebController(IDidDocumentGenerator generator, IIssuerService issuerService)
        { 
            _generator = generator;
            _issuerService = issuerService;
        }
        [HttpGet("issuers/{issuerid}/did.json")]
        [HttpGet("issuers/{issuerid}/.well-known/did.json")]
        public async Task<IActionResult> Get(string issuerid)
        {
            var uri = $"{Request.Scheme}://{Request.Host}/issuers/{issuerid}";
            var issuer = await _issuerService.GetIssuerAsync(issuerid, null, false);
            var keys = issuer.SigningKeys.Where(x => !x.Revoked)
                .Select(x => new Ed25519KeyPair
                {
                    Fragment = x.KeyFragment,
                    PublicKeyBase58 = x.PublicKey
                }).ToList();

            var doc = _generator.GenerateDidDocument(uri, keys);

            // 3) Return as JSON. Both System.Text.Json and Newtonsoft will work.
            return new JsonResult(doc);
        }

        [HttpGet("issuers/urn/uuid/{issuerid}/did.json")]
        [HttpGet("issuers/urn/uuid/{issuerid}/.well-known/did.json")]
        public async Task<IActionResult> GetUrn(string issuerid)
        {
            var uri = $"{Request.Scheme}://{Request.Host}/issuers/urn/uuid/{issuerid}";
            issuerid = $"urn:uuid:{issuerid}";
            var issuer = await _issuerService.GetIssuerAsync(issuerid, null, false);
            var keys = issuer.SigningKeys.Where(x => !x.Revoked)
                .Select(x => new Ed25519KeyPair
                {
                    Fragment = x.KeyFragment,
                    PublicKeyBase58 = x.PublicKey
                }).ToList();

            var doc = _generator.GenerateDidDocument(uri, keys);

            // 3) Return as JSON. Both System.Text.Json and Newtonsoft will work.
            return new JsonResult(doc);
        }
    }
}
