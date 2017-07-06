using FrameworkThree.Domain;

namespace FrameworkThree.Repository.Interface
{
    public interface IUnitOfWork
    {
        FrameworkThreeContext DbContext { get; }

        int Save();
    }
}
