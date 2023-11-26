using AuctionService.Data;
using AuctionService.DTO;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly AuctionDbContext _context;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        var auctions = await _context.Auctions
            .Include(x => x.Item)
            .OrderBy(x => x.Item.Make)
            .ToListAsync();
        return _mapper.Map<List<AuctionDto>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();
        return _mapper.Map<AuctionDto>(auction);


    }

    [HttpPost]
    public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto){
        var auction = _mapper.Map<Auction>(auctionDto);
        //TODO: add current user as seller
        auction.Seller = "test";
        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;
        if(!result) return BadRequest("Could not save changes to the DB");
        return CreatedAtAction(nameof(GetAuctionById), new {auction.Id}, _mapper.Map<AuctionDto>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id,UpdateAuctionDto updateAuctionDto){
        var auction = await _context.Auctions
        .Include(x=> x.Item).FirstOrDefaultAsync(x=> x.Id == id);
        if(auction == null) return NotFound();
        //TODO: check seller == username
        auction.Item.Make = updateAuctionDto.Make ?? updateAuctionDto.Make;
        auction.Item.Model = updateAuctionDto.Model ?? updateAuctionDto.Model;
        auction.Item.Color = updateAuctionDto.Color ?? updateAuctionDto.Color;
        auction.Item.Mileage = updateAuctionDto.Mileage ?? updateAuctionDto.Mileage;
        auction.Item.Year = updateAuctionDto.Year ?? updateAuctionDto.Year;

        var result = await _context.SaveChangesAsync() > 0;
        if(result) return Ok();
        return BadRequest("Problem saving changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction( Guid id){
        var auction = await _context.Auctions.FindAsync(id);
        if(auction == null) return NotFound();
        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync()>0;
        if(!result) return BadRequest();
        return Ok();

    }
}