using System.ComponentModel;
using System.Data.SqlClient;

namespace Database
{
    public interface INetElement : INotifyPropertyChanged
    {
        SqlCommand CreateCommandToAddToDatabase(int powerNetId);
        SqlCommand CreateCommandToUpdateInDatabase();
        SqlCommand CreateCommandToRemoveFromDatabase();
        int Id { get; set; }
    }
}
