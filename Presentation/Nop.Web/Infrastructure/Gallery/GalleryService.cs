using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Web.Infrastructure.Gallery
{
    public class GalleryService
    {
        private IRepository<Product> _productRepository;
        private IPictureService _pictureService;
        private ICacheManager _cacheManager;
        private MediaSettings _mediaSettings;
        private ILocalizationService _localizationService;
        private IWorkContext _workContext;
        private IWebHelper _webHelper;
        private IStoreContext _storeContext;

        public GalleryService(IRepository<Product> productRepository, IPictureService pictureService, ICacheManager cacheManager, MediaSettings mediaSettings, ILocalizationService localizationService, IWorkContext workContext, IWebHelper webHelper, IStoreContext storeContext)
        {
            _productRepository = productRepository;
            _pictureService = pictureService;
            _cacheManager = cacheManager;
            _mediaSettings = mediaSettings;
            _localizationService = localizationService;
            _workContext = workContext;
            _webHelper = webHelper;
            _storeContext = storeContext;
        }

        public ProductDetailsModel GetProduct(string name)
        {
            var product = _productRepository.Table.FirstOrDefault(x => x.Name == name.Trim());

            var model = new ProductDetailsModel();
            if (product != null)
            {
                model.Name = product.Name;
                const bool isAssociatedProduct = false;

                model.DefaultPictureZoomEnabled = false;
                //default picture
                var defaultPictureSize = _mediaSettings.ProductDetailsPictureSize;
                //prepare picture models
                var productPicturesCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_DETAILS_PICTURES_MODEL_KEY, product.Id, defaultPictureSize, isAssociatedProduct, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                var cachedPictures = _cacheManager.Get(productPicturesCacheKey, () =>
                {
                    var pictures = _pictureService.GetPicturesByProductId(product.Id);

                    var defaultPictureModel = new PictureModel()
                    {
                        ImageUrl = _pictureService.GetPictureUrl(pictures.FirstOrDefault(), defaultPictureSize, !isAssociatedProduct),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(pictures.FirstOrDefault(), 0, !isAssociatedProduct),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name),
                    };
                    //all pictures
                    var pictureModels = new List<PictureModel>();
                    foreach (var picture in pictures)
                    {
                        pictureModels.Add(new PictureModel()
                        {
                            ImageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(picture),
                            Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat.Details"), model.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat.Details"), model.Name),
                        });
                    }

                    return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
                });
                model.DefaultPictureModel = cachedPictures.DefaultPictureModel;
                model.PictureModels = cachedPictures.PictureModels;
            }
            return model;
        }
    }
}