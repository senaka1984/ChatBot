using System;

namespace ChatBot.Common.Domain.Base
{
    public abstract class Entity
    {
        private int? _requestedHashCode;
        public virtual Guid Id { get; protected set; }
        public DateTime CreatedDate { get; protected set; }
        public DateTime UpdatedDate { get; protected set; }
        public Guid CreatedById { get; protected set; }
        public Guid UpdatedById { get; protected set; }
        public bool IsDeleted { get; protected set; }


        // Rowversion
        protected Entity(Guid id)
        {
            Id = id;
            IsDeleted = false;
            SetCreatedDate();
            SetUpdatedDate();
        }

        protected Entity()
        {
            Id = Guid.NewGuid();
            IsDeleted = false;
            SetCreatedDate();
            SetUpdatedDate();
        }

        protected virtual void SetUpdatedDate()
            => UpdatedDate = DateTime.UtcNow;

        protected virtual void SetCreatedDate()
            => CreatedDate = DateTime.UtcNow;

        public void SetUpdatedBy(Guid id)
        {
            UpdatedById = id;
            SetUpdatedDate();
        }

        public void SetCreatedBy(Guid id)
        {
            CreatedById = id;
            SetCreatedDate();
        }

        public void SetIsDeleted(bool isDeleted)
        {
            IsDeleted = isDeleted;
        }

        public bool IsTransient()
        {
            return Id == default;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (GetType() != obj.GetType())
                return false;

            Entity item = (Entity)obj;

            if (item.IsTransient() || this.IsTransient())
                return false;
            else
                return item.Id == this.Id;
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

                return _requestedHashCode.Value;
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (Equals(left, null))
                return Equals(right, null);
            else
                return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }

    }
}