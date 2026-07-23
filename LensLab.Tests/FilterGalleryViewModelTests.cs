using System.Collections.Generic;
using System.Threading.Tasks;
using LensLab.Models;
using LensLab.Services;
using LensLab.ViewModels;
using Moq;
using Xunit;

namespace LensLab.Tests;

public class FilterGalleryViewModelTests
{
    private readonly Mock<IFilterLoaderService> _filterLoaderMock = new();
    private readonly Mock<IImageProcessingService> _imageProcessingMock = new();
    private readonly Mock<IDialogService> _dialogMock = new();
    private readonly Mock<IFileDialogService> _fileDialogMock = new();

    private FilterGalleryViewModel CreateSut()
    {
        return new FilterGalleryViewModel(_filterLoaderMock.Object, _imageProcessingMock.Object, _dialogMock.Object, _fileDialogMock.Object);
    }

    [Fact]
    public void SelectNextFilter_NavigatesThroughCollection()
    {
        // Arrange
        var sut = CreateSut();
        var item1 = new FilterPreviewItem { Filter = new FilterPreset { Name = "Preset 1" } };
        var item2 = new FilterPreviewItem { Filter = new FilterPreset { Name = "Preset 2" } };
        sut.Filters = new System.Collections.ObjectModel.ObservableCollection<FilterPreviewItem> { item1, item2 };
        sut.SelectedFilter = item1;

        // Act
        sut.SelectNextFilterCommand.Execute(null);

        // Assert
        Assert.Equal(item2, sut.SelectedFilter);
    }

    [Fact]
    public void SelectPreviousFilter_NavigatesThroughCollection()
    {
        // Arrange
        var sut = CreateSut();
        var item1 = new FilterPreviewItem { Filter = new FilterPreset { Name = "Preset 1" } };
        var item2 = new FilterPreviewItem { Filter = new FilterPreset { Name = "Preset 2" } };
        sut.Filters = new System.Collections.ObjectModel.ObservableCollection<FilterPreviewItem> { item1, item2 };
        sut.SelectedFilter = item2;

        // Act
        sut.SelectPreviousFilterCommand.Execute(null);

        // Assert
        Assert.Equal(item1, sut.SelectedFilter);
    }

    [Fact]
    public void FilterSelectedEvent_FiresWhenSelectedFilterChanges()
    {
        // Arrange
        var sut = CreateSut();
        var item = new FilterPreviewItem { Filter = new FilterPreset { Name = "Portra 400" } };
        sut.Filters = new System.Collections.ObjectModel.ObservableCollection<FilterPreviewItem> { item };

        FilterPreset? eventPreset = null;
        sut.FilterSelected += (sender, preset) => eventPreset = preset;

        // Act
        sut.SelectedFilter = item;

        // Assert
        Assert.NotNull(eventPreset);
        Assert.Equal("Portra 400", eventPreset.Name);
    }
}
