using AirsoftEquipmentTracker.Api.DTOs.Equipment;
using AirsoftEquipmentTracker.Core.Models;
using Mapster;

namespace AirsoftEquipmentTracker.Api.Configuration;

// Mapster-configuratie voor de mappings die niet vanzelf goed gaan.
// Wordt bij het opstarten gevonden via TypeAdapterConfig.GlobalSettings.Scan in Program.cs.
public class MapperProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Brand en Category zijn navigation properties; de DTO toont enkel hun naam
        config.NewConfig<EquipmentItem, EquipmentDto>()
            .Map(dest => dest.BrandName, src => src.Brand != null ? src.Brand.Name : string.Empty)
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty);

        // Request naar entity: velden die de server beheert, nooit overschrijven.
        // Dat gebeurt ook bij een PUT, waar de DTO op het bestaande item gemapt wordt.
        config.NewConfig<EquipmentRequestDto, EquipmentItem>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserId!)
            .Ignore(dest => dest.ImagePath!)
            .Ignore(dest => dest.Brand!)
            .Ignore(dest => dest.Category!);
    }
}
