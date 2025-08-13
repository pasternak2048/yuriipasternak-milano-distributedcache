using MILANO.DistributedCache.Server.Application.Cache;
using System.Security.Cryptography;
using System.Text;

namespace MILANO.DistributedCache.Server.Infrastructure.Cache
{
	/// <summary>
	/// Simple sharding strategy based on hash code modulo.
	/// Maps each key to a shard by computing its hash and applying modulo by shard count.
	/// </summary>
	public sealed class HashModuloShardingStrategy : IShardingStrategy
	{
		/// <inheritdoc />
		public int GetShardIndex(string key, int totalShards)
		{
			using var sha = SHA256.Create();
			var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(key));

			uint hashValue = BitConverter.ToUInt32(hashBytes, 0);
			return (int)(hashValue % totalShards);
		}
	}
}
