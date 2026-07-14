using EcoMeal.Client.Models;
using EcoMeal.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace EcoMeal.Client.Components.Pages.Orders;

public partial class Orders
{
    [Inject]
    private OrderService OrderService { get; set; } = default!;

    [Inject]
    private ReviewService ReviewService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private List<OrderGetModel>? MyOrders;

    private int? _reviewingOrderId;
    private int _newRating;
    private string? _newComment;
    private bool _submittingReview;
    private HashSet<int> _reviewedOrderIds = new();

    protected override async Task OnInitializedAsync()
    {
        MyOrders = await OrderService.GetMyOrderAsync();
    }

    private void StartReview(int orderId)
    {
        _reviewingOrderId = orderId;
        _newRating = 0;
        _newComment = null;
    }

    private void CancelReview()
    {
        _reviewingOrderId = null;
        _newRating = 0;
        _newComment = null;
    }

    private async Task SubmitReview(int businessId)
    {
        _submittingReview = true;
        StateHasChanged();

        var request = new ReviewRequest
        {
            BusinessId = businessId,
            Rating = _newRating,
            Review = _newComment
        };

        var success = await ReviewService.CreateReviewAsync(request);
        if (success)
        {
            Snackbar.Add("Review submitted!", Severity.Success);
            if (_reviewingOrderId.HasValue)
                _reviewedOrderIds.Add(_reviewingOrderId.Value);
            _reviewingOrderId = null;
            _newRating = 0;
            _newComment = null;
        }
        else
        {
            Snackbar.Add("Failed to submit review. This order may have already been reviewed.", Severity.Error);
        }

        _submittingReview = false;
    }

    private static MudBlazor.Color GetTimelineColor(string status)
    {
        return status?.ToLower() switch
        {
            "completed" or "picked up" => MudBlazor.Color.Success,
            "cancelled" => MudBlazor.Color.Error,
            "pending" => MudBlazor.Color.Warning,
            _ => MudBlazor.Color.Info
        };
    }

    private static string GetStatusIcon(string status)
    {
        return status?.ToLower() switch
        {
            "completed" or "picked up" => Icons.Material.Filled.CheckCircle,
            "cancelled" => Icons.Material.Filled.Cancel,
            "pending" => Icons.Material.Filled.HourglassTop,
            _ => Icons.Material.Filled.LocalShipping
        };
    }

    private static string GetStatusChipStyle(string status)
    {
        return status?.ToLower() switch
        {
            "completed" or "picked up" => "background: rgba(5, 150, 105, 0.15); color: #6EE7B7; font-weight: 600;",
            "cancelled" => "background: rgba(239, 68, 68, 0.15); color: #FCA5A5; font-weight: 600;",
            "pending" => "background: rgba(245, 158, 11, 0.15); color: #FCD34D; font-weight: 600;",
            _ => "background: rgba(59, 130, 246, 0.15); color: #93C5FD; font-weight: 600;"
        };
    }
}