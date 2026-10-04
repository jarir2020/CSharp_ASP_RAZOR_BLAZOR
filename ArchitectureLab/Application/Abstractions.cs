using ArchitectureLab.Domain;
using ArchitectureLab.Domain.Specifications;

namespace ArchitectureLab.Application;

public interface IRequest<out TResponse>
{
}

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> HandleAsync(
        TRequest request,
        CancellationToken cancellationToken);
}

public interface IRequestDispatcher
{
    Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(
        CourseId id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Course>> ListAsync(
        ICourseSpecification specification,
        CancellationToken cancellationToken);

    Task AddAsync(
        Course course,
        CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken);
}
