using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.WebServices
{
    public class CategoryDTO
    {
        private readonly IPictureService _pictureService;
        public CategoryDTO(){}
        public CategoryDTO(Category Cat)
        {
            _pictureService = EngineContext.Current.Resolve<IPictureService>();
            this.Id = Cat.Id;
            Name = Cat.Name;
            Image = _pictureService.GetPictureById(Cat.PictureId).PictureBinary;
            ShowOnHomePage = Cat.ShowOnHomePage;
            ParentId = Cat.ParentCategoryId;
            DisplayOrder = Cat.DisplayOrder;
        }
        public string Name { get; set; }

        public int Id { get; set; }

        public byte[] Image { get; set; }

        public bool ShowOnHomePage { get; set; }

        public int ParentId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
