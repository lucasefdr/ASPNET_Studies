namespace Studies.Domain.Shared;

public abstract class EntityBase
{
    protected EntityBase()
    {
    }

    public int Id { get; init; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public static bool operator ==(EntityBase? left, EntityBase? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(EntityBase? left, EntityBase? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        // Verifica se o objeto é nulo ou se é do tipo EntityBase
        if (obj is not EntityBase other)
            return false;

        // Se as referências de memória forem as mesmas, são o mesmo objeto
        if (ReferenceEquals(this, other))
            return true;

        // Verifica se os tipos concretos são os mesmos
        if (GetType() != other.GetType())
            return false;

        // Se o ID ainda não foi definido (uma entidade transiente), não pode ser igual
        if (Id.Equals(0) || other.Id.Equals(0))
            return false;

        // Compara os IDs
        return Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        // Combina o HashCode do tipo com o HashCode do ID.
        return HashCode.Combine(GetType(), Id);
    }
}