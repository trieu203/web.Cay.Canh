using AutoMapper;
using Cay.Canh.Web.HDT.Data;
using Cay.Canh.Web.HDT.ViewModel;

namespace Cay.Canh.Web.HDT.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Ánh xạ từ RegisterVM sang User
            CreateMap<RegisterVM, User>()
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));

            // Ánh xạ từ User sang UserVM (nếu có UserVM hoặc DTO để trả về thông tin người dùng)
            CreateMap<User, UserViewModel>();

            // Ánh xạ từ LoginVM sang User (chỉ khi đăng nhập)
            CreateMap<LoginVM, User>();

            CreateMap<RegisterVM, User>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // Xử lý ImageUrl sau trong Controller

        }
    }
}
