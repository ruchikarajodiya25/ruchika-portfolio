using AutoMapper;
using ServiceHubPro.Application.DTOs;
using ServiceHubPro.Domain.Entities;
using ServiceHubPro.Infrastructure.Entities;

namespace ServiceHubPro.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<RegisterDto, ApplicationUser>();

        // Tenant mappings
        CreateMap<Tenant, TenantDto>();
        CreateMap<CreateTenantDto, Tenant>();

        // Location mappings
        CreateMap<Location, LocationDto>();
        CreateMap<CreateLocationDto, Location>();

        // Customer mappings
        CreateMap<Customer, CustomerDto>();
        CreateMap<CreateCustomerDto, Customer>();
        CreateMap<UpdateCustomerDto, Customer>();

        // Service mappings
        CreateMap<Service, ServiceDto>();
        CreateMap<CreateServiceDto, Service>();
        CreateMap<UpdateServiceDto, Service>();

        // Appointment mappings
        CreateMap<Appointment, AppointmentDto>();
        CreateMap<CreateAppointmentDto, Appointment>();
        CreateMap<UpdateAppointmentDto, Appointment>();

        // WorkOrder mappings
        CreateMap<WorkOrder, WorkOrderDto>();
        CreateMap<CreateWorkOrderDto, WorkOrder>();

        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        // Invoice mappings
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<InvoiceItem, InvoiceItemDto>();

        // Notification mappings
        CreateMap<Notification, NotificationDto>();
    }
}
