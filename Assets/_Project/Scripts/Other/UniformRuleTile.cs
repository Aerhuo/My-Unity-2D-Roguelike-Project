using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Uniform Rule Tile", menuName = "2D/Tiles/Uniform Rule Tile")]
public class UniformRuleTile : RuleTile
{
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        // 获取当前坐标匹配到的 Rule
        TilingRule rule = GetMatchRule(position, tilemap);
        if (rule != null && rule.m_Output == TilingRuleOutput.OutputSprite.Random && rule.m_Sprites != null && rule.m_Sprites.Length > 1)
        {
            // 用真正的均匀分布 Hash 替代 Unity 偏心的 Perlin Noise
            float uniformRandom = GetUniformHash(position);
            int index = Mathf.Clamp(Mathf.FloorToInt(uniformRandom * rule.m_Sprites.Length), 0, rule.m_Sprites.Length - 1);
            
            tileData.sprite = rule.m_Sprites[index];
        }
    }

    private TilingRule GetMatchRule(Vector3Int position, ITilemap tilemap)
    {
        // 🌟 修复 CS0103 报错：创建一个局部的 Matrix4x4 变量传给 RuleMatches
        Matrix4x4 transform = Matrix4x4.identity;
        
        foreach (var rule in m_TilingRules)
        {
            if (RuleMatches(rule, position, tilemap, ref transform))
            {
                return rule;
            }
        }
        return null;
    }

    // 绝对均匀分布的 0.0 ~ 1.0 散列函数
    private float GetUniformHash(Vector3Int pos)
    {
        uint h = (uint)(pos.x * 0x85ebca6b ^ pos.y * 0xc2b2ae35 ^ pos.z * 0x27d4eb2d);
        h ^= h >> 16;
        h *= 0x85ebca6b;
        h ^= h >> 13;
        h *= 0xc2b2ae35;
        h ^= h >> 16;
        return (h & 0x00FFFFFF) / (float)0x01000000;
    }
}