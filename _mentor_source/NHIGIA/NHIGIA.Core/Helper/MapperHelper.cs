using AutoMapper;
using System.Collections.Generic;

namespace NHIGIA.Core.Helper
{
    public static class MapperHelper
    {
        //use for simple object
        public static TDestination Map<TSource, TDestination>(TSource source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            var mapper = config.CreateMapper();
            return mapper.Map<TDestination>(source);
        }

        //use for complex object : have child object/List<childObject> inside
        public static TDestination Map<TSource, TDestination, TProfile>(TSource source)
            where TProfile : Profile, new()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TProfile>();
            });

            var mapper = config.CreateMapper();
            return mapper.Map<TSource, TDestination>(source);
        }

        public static List<TDestination> MapList<TSource, TDestination>(List<TSource> source)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TSource, TDestination>();
            });

            var mapper = config.CreateMapper();
            return mapper.Map<List<TDestination>>(source);
        }

        public static List<TDestination> MapList<TSource, TDestination, TProfile>(List<TSource> source)
            where TProfile : Profile, new()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TProfile>();
            });

            var mapper = config.CreateMapper();
            return mapper.Map<List<TSource>, List<TDestination>>(source);
        }
    }
}
