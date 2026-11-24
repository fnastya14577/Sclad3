using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using SkladUchet1.Data;
using SkladUchet1.Models;
using SkladUchet1.Services;

namespace SkladUchet1.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _db = new();

    public ObservableCollection<Batch> Batches { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Batch> Alerts { get; } = new();
    public ObservableCollection<WriteOff> WriteOffs { get; } = new();

    private Batch? _selectedBatch;
    public Batch? SelectedBatch
    {
        get => _selectedBatch;
        set { _selectedBatch = value; OnPropertyChanged(); }
    }

    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set { _statusText = value; OnPropertyChanged(); }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); FilterBatches(); }
    }

    private string _reportText = "";
    public string ReportText
    {
        get => _reportText;
        set { _reportText = value; OnPropertyChanged(); }
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand WriteOffCommand { get; }
    public RelayCommand SaveReportCommand { get; }

    public MainViewModel()
    {
        _db.Database.EnsureCreated();
        RefreshCommand = new RelayCommand(_ => LoadData());
        WriteOffCommand = new RelayCommand(_ => DoWriteOff(), _ => SelectedBatch != null);
        SaveReportCommand = new RelayCommand(_ => SaveReport());
        LoadData();
    }

    private void LoadData()
    {
        Batches.Clear(); Alerts.Clear(); Products.Clear(); WriteOffs.Clear();

        var batchesFromDb = _db.Batches.Include(b => b.Product).ToList();

        foreach (var b in batchesFromDb)
        {
            b.CalculateStatus();
            Batches.Add(b);
            if (b.Remaining > 0 && b.Status != "В норме")
                Alerts.Add(b);
        }
        _db.SaveChanges();

        foreach (var p in _db.Products.ToList())
            Products.Add(p);

        foreach (var w in _db.WriteOffs.Include(w => w.Batch).OrderByDescending(w => w.Date).ToList())
            WriteOffs.Add(w);

        UpdateReport();
        FilterBatches();
    }

    private void FilterBatches()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _db.Batches.Include(b => b.Product).ToList()
            : _db.Batches.Include(b => b.Product).Where(b => b.Product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || b.BatchNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

        Batches.Clear();
        foreach (var b in filtered)
        {
            b.CalculateStatus();
            Batches.Add(b);
        }
        StatusText = $"Товаров: {Products.Count} | Партий: {Batches.Count} | Требуют внимания: {Alerts.Count}";
    }

    private void UpdateReport()
    {
        var totalProducts = Products.Count;
        var totalBatches = Batches.Count;
        var expired = Batches.Count(b => b.Status == "Просрочено");
        var expiringSoon = Batches.Count(b => b.Status == "Истекает скоро");

        ReportText = $"Всего товаров: {totalProducts}\n" +
                     $"Всего партий на складе: {totalBatches}\n" +
                     $"Просрочено: {expired}\n" +
                     $"Истекает скоро: {expiringSoon}\n\n" +
                     $"Общая стоимость остатков: {Batches.Sum(b => b.Remaining * (b.Product?.Price ?? 0)):C}";
    }

    private void SaveReport()
    {
        try
        {
            File.WriteAllText("report.txt", ReportText);
            MessageBox.Show("Отчет сохранен в файл report.txt", "Успех");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка");
        }
    }

    private void DoWriteOff()
    {
        if (SelectedBatch == null) return;

        var input = Microsoft.VisualBasic.Interaction.InputBox(
            $"Сколько списать из партии {SelectedBatch.BatchNumber}? (остаток: {SelectedBatch.Remaining})",
            "Списание", "1");

        if (!int.TryParse(input, out var qty) || qty <= 0) return;

        try
        {
            BatchService.WriteOff(SelectedBatch.Id, qty, 1,
                SelectedBatch.Status == "Просрочено" ? "Порча" : "Продажа");
            LoadData();
            MessageBox.Show("Списание выполнено!", "Успех");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}