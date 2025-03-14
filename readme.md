### Requirements: 

* WoW to scan AH data
* [AHDB](https://github.com/mooreatv/AuctionDB) to scan data ingame
* [AHDBapp](https://github.com/mooreatv/AHDBapp) to parse from lua to json and save to database.
* MYSQL Server for storing data

### Install
```
Git clone https://github.com/LienSimen/Disenchant-Classic
```
```
dotnet restore
```

From here simply add a .env file in root folder here with env configurations: 
```
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=yourpassword
DB_NAME=ahdb
```

```
dotnet run
```

Now listening on: http://localhost:5189

![Image](results.png)

The Disenchant query if you only needed that:
```
-- 1) Gather price for single enchanting material
    WITH material_prices AS (
        SELECT
        MIN(CASE WHEN i.name LIKE '%Illusion Dust%' THEN a.buyout / a.itemCount END) AS illusion_dust,
        MIN(CASE WHEN i.name LIKE '%Greater Eternal Essence%' THEN a.buyout / a.itemCount END) AS greater_eternal_essence,
        MIN(CASE WHEN i.name LIKE '%Lesser Eternal Essence%' THEN a.buyout / a.itemCount END) AS lesser_eternal_essence,
        MIN(CASE WHEN i.name LIKE '%Large Brilliant Shard%' THEN a.buyout / a.itemCount END) AS large_brilliant_shard,
        MIN(CASE WHEN i.name LIKE '%Small Brilliant Shard%' THEN a.buyout / a.itemCount END) AS small_brilliant_shard,
        MIN(CASE WHEN i.name LIKE '%Dream Dust%' THEN a.buyout / a.itemCount END) AS dream_dust,
        MIN(CASE WHEN i.name LIKE '%Greater Nether Essence%' THEN a.buyout / a.itemCount END) AS greater_nether_essence,
        MIN(CASE WHEN i.name LIKE '%Lesser Nether Essence%' THEN a.buyout / a.itemCount END) AS lesser_nether_essence,
        MIN(CASE WHEN i.name LIKE '%Large Radiant Shard%' THEN a.buyout / a.itemCount END) AS large_radiant_shard,
        MIN(CASE WHEN i.name LIKE '%Small Radiant Shard%' THEN a.buyout / a.itemCount END) AS small_radiant_shard,
        MIN(CASE WHEN i.name LIKE '%Vision Dust%' THEN a.buyout / a.itemCount END) AS vision_dust,
        MIN(CASE WHEN i.name LIKE '%Soul Dust%' THEN a.buyout / a.itemCount END) AS soul_dust,
        MIN(CASE WHEN i.name LIKE '%Lesser Mystic Essence%' THEN a.buyout / a.itemCount END) AS lesser_mystic_essence,
        MIN(CASE WHEN i.name LIKE '%Greater Mystic Essence%' THEN a.buyout / a.itemCount END) AS Greater_mystic_essence,
        MIN(CASE WHEN i.name LIKE '%Large Glowing Shard%' THEN a.buyout / a.itemCount END) AS large_glowing_shard,
        MIN(CASE WHEN i.name LIKE '%Small Glowing Shard%' THEN a.buyout / a.itemCount END) AS small_glowing_shard
        FROM auctions a
        JOIN items i ON i.id = a.itemId
        JOIN (
        SELECT MAX(id) AS latest_scan_id
        FROM scanmeta
        ) latest ON a.scanId = latest.latest_scan_id
        WHERE a.buyout > 0
    ),

    -- 2) Gather the min bid & min buyout for each item from the latest scan
    latest_auctions AS (
        SELECT
        a.itemId,
        a.ts,
        MIN(a.minBid)  AS min_bid,
        MIN(a.buyout)  AS min_buyout
        FROM auctions a
        JOIN (
        SELECT MAX(id) AS latest_scan_id
        FROM scanmeta
        ) latest ON a.scanId = latest.latest_scan_id
        WHERE a.buyout > 0
        GROUP BY a.itemId, a.ts
    ),

    -- 3) Calculate disenchant_value_raw for each item (based on level + armor/weapon)
    calc AS (
        SELECT
        i.id,
        i.name,
        i.MinLevel AS item_level,
        la.min_bid,
        la.min_buyout,
        CASE

            -- 31-35 ARMOR
            WHEN i.ClassID = 4 AND i.MinLevel BETWEEN 31 AND 35 THEN
                (0.75 * 1.5 * mp.vision_dust)
              + (0.20 * 1.5 * mp.greater_mystic_essence)
              + (0.05 * 1.0 *mp.large_glowing_shard)

            -- 31-35 WEAPON
            WHEN i.ClassID = 2 AND i.MinLevel BETWEEN 31 AND 35 THEN
                (0.75 * 1.5 * mp.greater_mystic_essence)
              + (0.20 * 1.5 * mp.vision_dust)
              + (0.05 * 1.0 *mp.large_glowing_shard)
            
            -- 36-40 ARMOR
            WHEN i.ClassID = 4 AND i.MinLevel BETWEEN 36 AND 40 THEN
                (0.75 * 3.5 * mp.vision_dust)
              + (0.20 * 1.5 * mp.lesser_nether_essence)
              + (0.05 * 1.0 * mp.small_radiant_shard)
            
            -- 36-40 WEAPONS
            WHEN i.ClassID = 2 AND i.MinLevel BETWEEN 36 AND 40 THEN
                (0.75 * 1.5 * mp.lesser_nether_essence)
              + (0.20 * 3.5 * mp.vision_dust)
              + (0.05 * 1.0 * mp.small_radiant_shard)

            -- 41-45 ARMOR
            WHEN i.ClassID = 4 AND i.MinLevel BETWEEN 41 AND 45 THEN
                (0.75 * 1.5 * mp.dream_dust)
            + (0.20 * 1.5 * mp.greater_nether_essence)
            + (0.05 * 1.0 *mp.large_radiant_shard)

            -- 41-45 WEAPON
            WHEN i.ClassID = 2 AND i.MinLevel BETWEEN 41 AND 45 THEN
                (0.75 * 1.5 * mp.greater_nether_essence)
              + (0.20 * 1.5 * mp.dream_dust)
              + (0.05 * 1.0 *mp.large_radiant_shard)
            
            -- 46-50 ARMOR
            WHEN i.ClassID = 4 AND i.MinLevel BETWEEN 46 AND 50 THEN
                (0.75 * 3.5 * mp.dream_dust)
              + (0.20 * 1.5 * mp.lesser_eternal_essence)
              + (0.05 * 1.0 * mp.small_brilliant_shard)

            -- 46-50 WEAPONS
            WHEN i.ClassID = 2 AND i.MinLevel BETWEEN 46 AND 50 THEN
                (0.75 * 1.5 * mp.lesser_eternal_essence)
              + (0.20 * 3.5 * mp.dream_dust)
              + (0.05 * 1.0 * mp.small_brilliant_shard)

            -- 51-55 ARMOR
            WHEN i.ClassID = 4 AND i.MinLevel BETWEEN 51 AND 55 THEN
                (0.75 * 1.5 * mp.illusion_dust)
              + (0.20 * 1.5 * mp.greater_eternal_essence)
              + (0.05 * 1.0 * mp.large_brilliant_shard)

            -- 51-55 WEAPONS
            WHEN i.ClassID = 2 AND i.MinLevel BETWEEN 51 AND 55 THEN
                (0.75 * 1.5 * mp.greater_eternal_essence)
            + (0.20 * 1.5 * mp.illusion_dust)
            + (0.05 * 1.0 * mp.large_brilliant_shard)

            -- 56-60 ARMOR
            WHEN i.ClassID = 4 AND i.MinLevel BETWEEN 56 AND 60 THEN
                (0.75 * 3.5 * mp.illusion_dust)
              + (0.20 * 2.5 * mp.greater_eternal_essence)
              + (0.05 * 1.0 * mp.large_brilliant_shard)

            -- 56-60 WEAPONS
            WHEN i.ClassID = 2 AND i.MinLevel BETWEEN 56 AND 60 THEN
                (0.75 * 2.5 * mp.greater_eternal_essence)
              + (0.20 * 3.5 * mp.illusion_dust)
              + (0.05 * 1.0 * mp.large_brilliant_shard)

            ELSE 0
        END AS disenchant_value_raw
        FROM items i
        JOIN latest_auctions la ON i.id = la.itemId
        CROSS JOIN material_prices mp
        WHERE i.Rarity = 2
        AND i.MinLevel BETWEEN 31 AND 60
        AND i.ClassID IN (2, 4)
    )

    -- 4) Final SELECT: show everything, plus the profit columns
    SELECT
    c.id,
    c.name,
    c.item_level,
    c.min_bid    AS min_bid_raw,
    c.min_buyout AS min_buyout_raw,
    c.disenchant_value_raw,
    (c.disenchant_value_raw - c.min_bid)    AS profit_vs_minBid,
    (c.disenchant_value_raw - c.min_buyout) AS profit_vs_buyout
    FROM calc c
    -- Show only items with a positive profit:
    WHERE (c.disenchant_value_raw > c.min_buyout)
    ORDER BY (c.disenchant_value_raw - c.min_buyout) DESC;
    ";
```
