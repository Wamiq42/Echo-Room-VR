# Textures — sources, IDs, and download code

Two texture sources for Echo Room props. Prefer **ADG** when matching the maze; use **Poly Haven** for
materials ADG doesn't cover (metal, wood, brass, etc.).

## 1. ADG_Textures (already in the project)

Location: `Assets/ADG_Textures/walls_vol1/` — **18 full PBR wall sets** `wall01`…`wall18`.
Each folder has: `<name>_Diffuse.tga`, `_Normal.tga`, `_Metallic.tga`, `_Height.tga`, `_Ambient_Occlusion.tga`
(a couple are `.tif` for Diffuse). These are the **maze wall** textures — props should match/complement them.

- The **lever stone base** uses `wall03`. Swap to another `wallNN` to match whatever wall the maze ends up using.
- No separate roughness map (has Metallic + AO + Height). For stone, set Smoothness low (~0.2–0.3) as a constant.

## 2. Poly Haven (free CC0) — download via script, NO add-on

Internet works inside Blender's `execute_blender_code`. Query the API, then download the map URLs.
The API returns keys like `Diffuse`, `nor_gl` (OpenGL normal — use this for Unity), `nor_dx`, `Rough`, `arm`
(AO+Rough+Metal packed), `AO`, `Displacement`. Each is `[<res>][<format>]['url']`, e.g. `["Diffuse"]["2k"]["jpg"]["url"]`.

```python
import urllib.request, json, os
def ph_api(aid):
    req = urllib.request.Request("https://api.polyhaven.com/files/"+aid, headers={"User-Agent":"Mozilla/5.0"})
    return json.loads(urllib.request.urlopen(req, timeout=25).read().decode())
def ph_dl(url, dest):
    req = urllib.request.Request(url, headers={"User-Agent":"Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=120) as r, open(dest, "wb") as f:
        f.write(r.read())
def ph_grab(aid, folder, maps=("Diffuse","nor_gl","arm","Rough"), res="2k"):
    """Download an asset's maps as <aid>_<map>.jpg into folder. Returns {map: path}."""
    os.makedirs(folder, exist_ok=True)
    d = ph_api(aid); got = {}
    for m in maps:
        node = d.get(m, {}).get(res, {}).get("jpg")
        if node and node.get("url"):
            dest = os.path.join(folder, f"{aid}_{m}.jpg")
            ph_dl(node["url"], dest); got[m] = dest
    return got
```

Download prop textures to `Assets/_EchoRoom/Textures/<PropName>/`.

### Texture IDs used so far

| Material | Poly Haven ID | Notes |
|---|---|---|
| Metal (iron, rusty) | `rusty_metal_03` | bands/rivets/rod; metallic = 1 in Unity |
| Wood (weathered) | `weathered_planks` | dome + grip; metallic = 0 |
| Stone (base) | ADG `wall03` | matches the maze; metallic = 0 |
| Brass (ring) | `rusty_metal_03` + gold `_BaseColor` tint | metallic = 1, smoothness ~0.6 |

To find more IDs: browse polyhaven.com/textures (the URL slug is the `<id>`), or guess common names and
check `ph_api(id)` succeeds. Candidates that exist include `weathered_planks`, `wood_planks`, `plank_flooring_02`,
`dark_wood`. Metal: `rusty_metal_03`, `metal_plate`. Always confirm via the API before downloading.

## ARM map channel mapping (Poly Haven `arm`)

`arm` packs **R=AO, G=Roughness, B=Metallic**. In a Blender node graph: `SeparateColor` → Green→Roughness,
Blue→Metallic. (In Unity these are set as constants per material instead — see `unity.md`.)
