using AMANC_Inventory.Core.Architecture;
using System.Windows.Input;

namespace AMANC_Inventory.Inventory.ViewModels
{
    public partial class InventoryViewModel
    {
        private int _currentPage = 1;
        private int _totalPages = 1;

        public ICommand FirstPageCommand { get; private set; } = null!;
        public ICommand PreviousPageCommand { get; private set; } = null!;
        public ICommand NextPageCommand { get; private set; } = null!;
        public ICommand LastPageCommand { get; private set; } = null!;
        public ICommand GoToPageCommand { get; private set; } = null!;

        private void InicializarComandosPaginacion()
        {
            FirstPageCommand = new RelayCommand(_ => CurrentPage = 1, _ => CurrentPage > 1);
            PreviousPageCommand = new RelayCommand(_ => CurrentPage--, _ => CurrentPage > 1);
            NextPageCommand = new RelayCommand(_ => CurrentPage++, _ => CurrentPage < TotalPages);
            LastPageCommand = new RelayCommand(_ => CurrentPage = TotalPages, _ => CurrentPage < TotalPages);
            GoToPageCommand = new RelayCommand(p => { if (p != null && int.TryParse(p.ToString(), out int page)) CurrentPage = page; });
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    (FirstPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PreviousPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LastPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (SetProperty(ref _totalPages, value))
                {
                    (NextPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LastPageCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
    }
}