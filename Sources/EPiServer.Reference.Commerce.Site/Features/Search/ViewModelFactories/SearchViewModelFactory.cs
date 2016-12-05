﻿using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using Mediachase.Search;
using EPiServer.Reference.Commerce.Site.Features.Search.Services;
using EPiServer.Reference.Commerce.Site.Features.Search.ViewModels;
using Mediachase.Commerce;

namespace EPiServer.Reference.Commerce.Site.Features.Search.ViewModelFactories
{
    public class SearchViewModelFactory
    {
        private readonly ISearchService _searchService;
        private readonly LocalizationService _localizationService;
        private readonly ICurrentMarket _currentMarket;

        public SearchViewModelFactory(LocalizationService localizationService, ISearchService searchService, ICurrentMarket currentMarket)
        {
            _searchService = searchService;
            _localizationService = localizationService;
            _currentMarket = currentMarket;
        }

        public virtual SearchViewModel<T> Create<T>(T currentContent, FilterOptionViewModel viewModel) where T : IContent
        {
            if (viewModel.Q != null && (viewModel.Q.StartsWith("*") || viewModel.Q.StartsWith("?")))
            {
                return new SearchViewModel<T>
                {
                    CurrentContent = currentContent,
                    FilterOption = viewModel,
                    HasError = true,
                    ErrorMessage = _localizationService.GetString("/Search/BadFirstCharacter")
                };
            }

            var customSearchResult = _searchService.Search(currentContent, viewModel);

            viewModel.TotalCount = customSearchResult.SearchResult != null ? customSearchResult.SearchResult.TotalCount : 0;
            viewModel.FacetGroups = customSearchResult.FacetGroups.ToList();

            viewModel.Sorting = _searchService.GetSortOrder().Select(x => new SelectListItem
            {
                Text = _localizationService.GetString("/Category/Sort/" + x.Name),
                Value = x.Name.ToString(),
                Selected = string.Equals(x.Name.ToString(), viewModel.Sort)
            });
            var currentMarketCode = _currentMarket.GetCurrentMarket().MarketId;
            customSearchResult.ProductViewModels =
                customSearchResult.ProductViewModels.Where(item => item.IsAvailable);

            return new SearchViewModel<T>
            {
                CurrentContent = currentContent,
                ProductViewModels = customSearchResult.ProductViewModels,
                Facets = customSearchResult.SearchResult != null ? customSearchResult.SearchResult.FacetGroups : new ISearchFacetGroup[0],
                FilterOption = viewModel
            };
        }

    }
}