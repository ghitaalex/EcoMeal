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
    private Dictionary<int, (int Rating, string? Comment)> _submittedReviews = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            MyOrders = await OrderService.GetMyOrderAsync();
            StateHasChanged();
        }
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

    private async Task SubmitReview(int orderId, int businessId)
    {
        _submittingReview = true;
        StateHasChanged();

        var request = new ReviewRequest
        {
            OrderId = orderId,
            BusinessId = businessId,
            Rating = _newRating,
            Review = _newComment
        };

        var success = await ReviewService.CreateReviewAsync(request);
        if (success)
        {
            Snackbar.Add("Review submitted!", Severity.Success);
            _reviewedOrderIds.Add(orderId);
            _submittedReviews[orderId] = (_newRating, _newComment);
            _reviewingOrderId = null;
            _newRating = 0;
            _newComment = null;

            // Mark the order as reviewed so it persists if re-fetched
            var order = MyOrders?.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
                order.IsReviewed = true;
        }
        else
        {
            // If submission fails, the order was likely already reviewed
            var order = MyOrders?.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
                order.IsReviewed = true;
            _reviewingOrderId = null;
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
            "completed" or "picked up" => "background: var(--chip-bg); color: var(--accent-light); font-weight: 600;",
            "cancelled" => "background: var(--status-cancelled-bg); color: #FCA5A5; font-weight: 600;",
            "pending" => "background: var(--status-pending-bg); color: #FCD34D; font-weight: 600;",
            _ => "background: rgba(59, 130, 246, 0.15); color: #93C5FD; font-weight: 600;"
        };
    }
}