"""Builds content/[CP] ValleyOpus from slice_data.py, plus its placeholder art.

Run from the repository root:  python3 tools/basepack/build_base_pack.py [--art-root /mnt/project-files/art-stockpile]

The pack is committed, so the game and CI never run this; rerun it after editing slice_data.py. Base-pack rows
write the CharlesDR.ValleyOpus_ prefix literally: in this pack {{ModId}} is CharlesDR.ValleyOpus.CP (doc 12)."""

import argparse
import json
import os
from collections import OrderedDict

import slice_data as D

P = D.P
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PACK = os.path.join(ROOT, "content", "[CP] ValleyOpus")
TEXTURE = "Mods/CharlesDR.ValleyOpus/Objects"


def q(local):
    return "(O)" + P + local


def build(art_root):
    i18n = OrderedDict()
    objects, big, crafting = OrderedDict(), OrderedDict(), OrderedDict()
    registries = OrderedDict((name, OrderedDict()) for name in [
        "Primes", "Operations", "Conditions", "Apparatus", "Substances", "Recipes",
        "Decomposition/TagWeights", "Decomposition/CategoryDefaults", "Decomposition/Overrides", "Decomposition/Settings",
        "Products", "Effects", "XpSources", "Codex/Pages", "Codex/Lore", "Milestones"])
    sprites = []  # (local id, kind, colour, rune, flask row)
    prime_colour = {p[0]: p[3] for p in D.PRIMES}
    used_runes = set()

    def rune_for(wanted):
        if wanted and wanted not in used_runes:
            used_runes.add(wanted)
            return wanted
        for r in range(5):
            for c in range(15):
                if (r, c) not in used_runes:
                    used_runes.add((r, c))
                    return (r, c)
        raise RuntimeError("out of runes")

    for p in D.PRIMES:
        used_runes.add(p[5])

    def add_object(local, display, gloss, sort, category, price, tags, colour, rune, flask_row, extra=None):
        index = len(sprites)
        sprites.append((local, colour, rune, flask_row))
        i18n[f"item.{local}.name"] = display
        i18n[f"item.{local}.desc"] = gloss
        row = OrderedDict([
            ("Name", f"{P}{sort}_{local}"),
            ("DisplayName", "{{i18n:item." + local + ".name}}"),
            ("Description", "{{i18n:item." + local + ".desc}}"),
            ("Type", "Basic"),
            ("Category", category),
            ("Price", price),
            ("Texture", TEXTURE),
            ("SpriteIndex", index),
            ("Edibility", -300),
            ("ContextTags", tags),
        ])
        if extra:
            row.update(extra)
        objects[P + local] = row

    # Primes
    for local, display, group, colour, order, rune, price, gloss in D.PRIMES:
        registries["Primes"][P + local] = OrderedDict([
            ("ItemId", q(local)), ("Group", group), ("Color", colour), ("SortOrder", order),
            ("NameKey", "item." + local + ".name"), ("GlossKey", "item." + local + ".desc")])
        add_object(local, display, gloss, D.SORT_PRIME + order, -16, price,
                   [D_REAGENT, "valleyopus_prime", "valleyopus_group_" + group.lower()], colour, rune, 1)

    # Operations
    for op in D.OPERATIONS:
        registries["Operations"][P + op] = {"NameKey": "operation." + op}
        i18n["operation." + op] = op

    # Conditions (doc 11 §2: flowers within range 2; darkness lives in the test fixture pack)
    registries["Conditions"][P + "Flowers"] = OrderedDict([
        ("Kind", "Adjacency"), ("Adjacency", {"Tag": "flower_item", "Directions": "All", "Range": 2}),
        ("NameKey", "condition.Flowers"), ("HintKey", "condition.Flowers.hint")])
    i18n["condition.Flowers"] = "Flowers nearby"
    i18n["condition.Flowers.hint"] = "Something here likes the smell of flowers."

    # Apparatus
    for local, display, kind, sprite, desc in D.APPARATUS:
        row = OrderedDict([("ItemId", "(BC)" + P + local), ("Kind", kind)])
        if local == "ExperimentTable":
            row.update([("NeighbourStates", [{"Id": "Table", "Test": {}}]),
                        ("Operations", [{"Operation": P + op, "States": ["Table"]} for op in D.OPERATIONS]),
                        ("TrayCapacity", 2), ("Trigger", "Hand"), ("Automation", "None"), ("Chaining", False)])
        elif local == "Alembic":
            row.update([("NeighbourStates", [{"Id": "Cold", "Test": {"Heat": False}}, {"Id": "Heated", "Test": {"Heat": True}}]),
                        ("Operations", [{"Operation": P + "Dissolution", "States": ["Cold"]}, {"Operation": P + "Distillation", "States": ["Heated"]}])])
        else:
            ops = [op for op, places in D.OP_APPARATUS.items() if any(a == local for a, _ in places)]
            row.update([("NeighbourStates", [{"Id": "Open", "Test": {}}]),
                        ("Operations", [{"Operation": P + op, "States": ["Open"]} for op in ops])])
        if local == "Athanor":
            row["Decompose"] = {"Concentration": 2, "ResidueRate": {"Count": 1, "Chance": 0.1}, "Minutes": 120}
            row["Heat"] = {"RequiresFlag": "Kindled", "MaxConsumers": 2}
        if local == "Calcinator":
            row["Decompose"] = {"ResidueRate": {"Count": 1, "Chance": 0.35}, "Minutes": 30}
            row["Fuel"] = {"Item": "(O)382", "Capacity": 10, "PerRun": 1}
        row["FeedbackStyle"] = "Mechanical" if kind == "Engineered" else "Arcane"
        row["NameKey"] = "apparatus." + local + ".name"
        registries["Apparatus"][P + local] = row
        i18n["apparatus." + local + ".name"] = display
        i18n["apparatus." + local + ".desc"] = desc
        big[P + local] = OrderedDict([
            ("Name", P + local), ("DisplayName", "{{i18n:apparatus." + local + ".name}}"),
            ("Description", "{{i18n:apparatus." + local + ".desc}}"), ("Price", 100), ("Fragility", 0),
            ("CanBePlacedOutdoors", True), ("CanBePlacedIndoors", True), ("IsLamp", False),
            ("Texture", "TileSheets/Craftables"), ("SpriteIndex", sprite),
            ("ContextTags", ["valleyopus_apparatus", "valleyopus_apparatus_" + local.lower()])])

    def recipe_row(local, inputs, op, table_only, tier, output=None):
        counts = OrderedDict()
        for i in inputs:
            counts[i] = counts.get(i, 0) + 1
        places = [] if table_only else D.OP_APPARATUS[op]
        if local == "FlammaViva":
            places = [("Athanor", "Open")]  # kindling: only the Athanor itself is lit by it (doc 13 §2)
        conditions = []
        if op in D.FLOWER_BONUS_OPS and not table_only:
            conditions.append({"ConditionId": P + "Flowers", "Use": "Bonus", "Effect": {"QualitySteps": 1}})
        return OrderedDict([
            ("Inputs", [OrderedDict([("Item", P + k)] + ([("Count", n)] if n > 1 else [])) for k, n in counts.items()]),
            ("Operation", P + op),
            ("ApparatusIds", [P + "ExperimentTable"] + [P + a for a, _ in places]),
            ("NeighbourStates", ["Table"] + sorted({s for _, s in places})),
            ("Conditions", conditions),
            ("Output", {"Id": P + (output or local)}),
            ("Quality", {"Mode": "InheritMin"}),
            ("Minutes", 120 if tier == 1 else 240),
        ])

    def product_extra(local):
        prod = D.PRODUCTS.get(local)
        if not prod:
            return None, -16, [D_REAGENT, "valleyopus_substance"]
        extra = OrderedDict()
        tags = ["valleyopus_product", "valleyopus_substance"]
        if "Edible" in prod:
            e = prod["Edible"]
            extra["Edibility"] = e["Edibility"]
            extra["IsDrink"] = e.get("IsDrink", False)
            if e.get("Effects"):
                extra["Buffs"] = [{"Id": P + eff, "BuffId": P + eff, "Duration": 240} for eff in e["Effects"]]
        if "Dye" in prod:
            tags += ["color_" + prod["Dye"], "dye_strong"]
        if local in ("RosMaialis",):
            tags.append(D_REAGENT)  # also a Vigor input
        return extra, prod["Category"], tags

    def add_substance(local, display, inputs, op, cls, table_only, rune, gloss, tier, sort):
        sub = OrderedDict([("Class", cls), ("Tier", tier)])
        if cls == "Item":
            sub["ItemId"] = q(local)
            sub["Bottleable"] = True
        sub["NameKey"] = "item." + local + ".name"
        sub["GlossKey"] = "item." + local + ".desc"
        registries["Substances"][P + local] = sub
        if inputs:
            registries["Recipes"][P + local] = recipe_row(local, inputs, op, table_only, tier)
        if cls == "Item":
            known = [prime_colour[i] for i in inputs if i in prime_colour]
            colour = blend(known) if known else "#6E6A66"
            prime_colour.setdefault(local, colour)  # Tier 2 blends its inputs' colours
            extra, category, tags = product_extra(local)
            add_object(local, display, gloss, sort, category, 1, tags, colour, rune_for(rune), 1 if tier == 1 else 2, extra)
            registries["Decomposition/Overrides"][q(local)] = (
                {"Profile": profile_of(inputs), "Units": 1} if inputs else {"Refuse": True})
        else:
            i18n[f"item.{local}.name"] = display
            i18n[f"item.{local}.desc"] = gloss

    tier1_inputs = {t[0]: t[2] for t in D.TIER1}

    def flatten(name):
        if name in prime_colour and name not in tier1_inputs and any(p[0] == name for p in D.PRIMES):
            return [name]
        if name in tier1_inputs:
            return [x for i in tier1_inputs[name] for x in flatten(i)]
        return ["Terra"]  # Caput Mortuum: dead earth

    def profile_of(inputs):
        prof = OrderedDict()
        for i in (x for name in inputs for x in flatten(name)):
            prof[P + i] = prof.get(P + i, 0) + 1
        return prof

    for n, (local, display, inputs, op, cls, table_only, rune, gloss) in enumerate(D.TIER1):
        add_substance(local, display, inputs, op, cls, table_only, rune, gloss, 1, D.SORT_T1 + n)
    cm_local, cm_display, cm_gloss = D.CAPUT_MORTUUM
    add_substance(cm_local, cm_display, [], None, "Item", False, (4, 14), cm_gloss, 1, D.SORT_RESIDUE)
    for n, (local, display, inputs, op, rune, gloss) in enumerate(D.TIER2):
        add_substance(local, display, inputs, op, "Item", False, rune, gloss, 2, D.SORT_T2 + n)

    # Tools (doc 11: Satchel and Lens; Data/Objects items, not tools)
    for local, display, desc, sprite_kind in [
            ("Satchel", "Reagent Satchel", "Holds reagents in its own pages. Primes and inputs you pick up go straight in. Click an apparatus while holding it to load a recipe.", "satchel"),
            ("Lens", "Assayer's Lens", "Hold it and click an item, a chest or a machine to see what it is made of.", "lens")]:
        index = len(sprites)
        sprites.append((local, sprite_kind, None, None))
        i18n[f"item.{local}.name"] = display
        i18n[f"item.{local}.desc"] = desc
        objects[P + local] = OrderedDict([
            ("Name", P + local), ("DisplayName", "{{i18n:item." + local + ".name}}"),
            ("Description", "{{i18n:item." + local + ".desc}}"), ("Type", "Basic"), ("Category", 0), ("Price", 0),
            ("Texture", TEXTURE), ("SpriteIndex", index), ("Edibility", -300), ("ExcludeFromShippingCollection", True),
            ("ExcludeFromFishingCollection", True), ("ExcludeFromRandomSale", True),
            ("ContextTags", ["valleyopus_tool", "not_giftable", "not_placeable"])])

    # Decomposition tables
    for cat, weights in D.CATEGORY_DEFAULTS.items():
        registries["Decomposition/CategoryDefaults"][cat] = {"Weights": {P + k: v for k, v in weights.items()}}
    for tag, weights in D.TAG_WEIGHTS.items():
        registries["Decomposition/TagWeights"][tag] = {"Weights": {P + k: v for k, v in weights.items()}}
    for tag, weights in D.AETHER_SOURCES.items():
        registries["Decomposition/TagWeights"][tag] = {"Weights": {P + k: v for k, v in weights.items()}, "AetherSource": True}
    registries["Decomposition/Overrides"][q("CaputMortuum")] = {"Refuse": True}
    registries["Decomposition/Settings"]["Default"] = OrderedDict([
        ("K", 1.0), ("E", 0.5), ("QualityBonus", 0.1), ("Threshold", 0.10), ("ExchangeRatio", 0.8),
        ("DefaultResidue", {"Count": 1, "Chance": 0.25}), ("ResidueItem", q("CaputMortuum"))])

    # Products and effects (doc 15)
    for local, prod in D.PRODUCTS.items():
        uses = []
        if "Edible" in prod:
            e = prod["Edible"]
            uses.append(OrderedDict([("Kind", "Edible"), ("Energy", round(e["Edibility"] * 2.5)),
                                     ("Health", round(e["Edibility"] * 1.125)), ("IsDrink", e.get("IsDrink", False)),
                                     ("Effects", [P + x for x in e.get("Effects", [])])]))
        if "Fertilizer" in prod:
            uses.append(OrderedDict([("Kind", "Fertilizer")] + list(prod["Fertilizer"].items())))
        if "Dye" in prod:
            uses.append({"Kind": "Material", "System": "Dye", "Ref": prod["Dye"]})
        price = {"Mode": "Fixed", "Value": prod["FixedPrice"]} if "FixedPrice" in prod else {"Mode": "Derived"}
        registries["Products"][P + local] = OrderedDict([
            ("Family", prod["Family"]), ("Uses", uses), ("Price", price),
            ("CategoryRole", "Artisan" if prod["Category"] == -26 else "Fertilizer")])
    registries["Effects"][P + "UndinesGift"] = OrderedDict([
        ("Kind", "Custom"), ("Handler", "UndinesGift"), ("Params", {"Chance": "0.5"}), ("Duration", 240), ("Tier", "Weak"),
        ("NameKey", "effect.UndinesGift.name"), ("DescKey", "effect.UndinesGift.desc")])
    i18n["effect.UndinesGift.name"] = "Undine's Gift"
    i18n["effect.UndinesGift.desc"] = "Half of your waterings use no water."

    # XP sources (doc 05)
    for key, row in [
            ("Discovery", {"Event": "Discovery", "Amount": 100, "TierScale": True, "OncePer": "Recipe"}),
            ("EffectDiscovery", {"Event": "EffectDiscovery", "Amount": 25, "OncePer": "Effect"}),
            ("HandCollect", {"Event": "HandCollect", "Amount": 2, "CapPerAction": 20}),
            ("AutoCollect", {"Event": "AutoCollect", "Amount": 2, "CapPerAction": 20, "Scale": 0.25}),
            ("Assay", {"Event": "Assay", "Amount": 5, "OncePer": "Item"}),
            ("TriedKey", {"Event": "TriedKey", "Amount": 3, "OncePer": "Key"})]:
        registries["XpSources"][P + key] = row

    # Codex pages, lore and milestones (doc 07, doc 16 §6)
    for order, (key, title) in enumerate([("Primes", "The Primes"), ("Tier1", "The First Work"), ("Tier2", "The Second Work"),
                                          ("Apparatus", "Apparatus"), ("Unlocks", "The Workshop")]):
        registries["Codex/Pages"][P + key] = {"Group": key, "SortOrder": order * 10, "TitleKey": "codex.page." + key}
        i18n["codex.page." + key] = title
    for local, *_ in D.PRIMES:
        registries["Codex/Lore"][P + local + "_1"] = {"TargetId": q(local), "Threshold": 1, "TextKey": "lore." + local + ".1"}
    lore = {
        "Ignis": "Cyprian's note: \"Everything that burns gives some of this back.\"",
        "Aqua": "Cyprian's note: \"Even stone weeps a little Aqua, if you ask it right.\"",
        "Terra": "Cyprian's note: \"The commonest Prime, and the most patient.\"",
        "Aer": "Cyprian's note: \"Flowers are mostly breath.\"",
        "Sal": "Cyprian's note: \"The sea is full of it. So are tears.\"",
        "Sulphur": "Cyprian's note: \"Eggs, sap and anything that stinks.\"",
        "Mercurius": "Cyprian's note: \"It runs out of every vessel but one.\"",
        "Aether": "Cyprian's note: \"I have held it three times. Each time something rare was given up for it.\"",
    }
    for k, v in lore.items():
        i18n["lore." + k + ".1"] = v
    for key, trigger, celebration, text in [
            ("FirstTier1", "FirstOfTier:1", "HoldUp", "Your first Tier 1 substance!"),
            ("FirstTier2", "FirstOfTier:2", "HoldUp", "A Tier 2 compound: two substances made one."),
            ("Kindled", "Process:" + P + "FlammaViva", "HoldUp", "The Athanor is lit, and will stay lit."),
            ("Tier1Complete", "TierComplete:1", "Flourish", "Every cell of the First Work is filled.")]:
        registries["Milestones"][P + key] = {"Trigger": trigger, "Celebration": celebration, "TextKey": "milestone." + key}
        i18n["milestone." + key] = text

    # Crafting recipes (1.6 format: ingredients/unused/yield/big craftable/unlock/display name)
    for local, (ingredients, is_big) in D.CRAFTING.items():
        crafting[P + local] = f"{ingredients}/Home/{P}{local}/{'true' if is_big else 'false'}/none/"

    return registries, objects, big, crafting, i18n, sprites


D_REAGENT = "valleyopus_reagent"


def blend(colours):
    rgb = [tuple(int(c[i:i + 2], 16) for i in (1, 3, 5)) for c in colours]
    avg = [round(sum(c[i] for c in rgb) / len(rgb)) for i in range(3)]
    return "#%02X%02X%02X" % tuple(avg)


def write_json(path, value, header=None):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    text = json.dumps(value, indent=2, ensure_ascii=False)
    with open(path, "w", encoding="utf-8") as f:
        if header:
            f.write("".join("// " + line + "\n" for line in header))
        f.write(text + "\n")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--art-root", default="/mnt/project-files/art-stockpile")
    args = parser.parse_args()

    registries, objects, big, crafting, i18n, sprites = build(args.art_root)
    header = ["Generated by tools/basepack/build_base_pack.py from slice_data.py; edit those and rerun.",
              "Base-pack rows write the CharlesDR.ValleyOpus_ prefix literally (doc 12): {{ModId}} here is the CP pack's ID."]

    write_json(os.path.join(PACK, "data", "registries.json"), {"Changes": [
        {"Action": "EditData", "Target": "CharlesDR.ValleyOpus/" + name, "Entries": rows}
        for name, rows in registries.items() if rows]}, header)
    write_json(os.path.join(PACK, "data", "items.json"), {"Changes": [
        {"Action": "EditData", "Target": "Data/Objects", "Entries": objects},
        {"Action": "EditData", "Target": "Data/BigCraftables", "Entries": big},
        {"Action": "EditData", "Target": "Data/CraftingRecipes", "Entries": crafting},
    ]}, header)
    write_json(os.path.join(PACK, "i18n", "default.json"), i18n)
    write_json(os.path.join(PACK, "content.json"), OrderedDict([
        ("Format", "2.4.0"),
        ("Changes", [
            {"Action": "Load", "Target": "Mods/CharlesDR.ValleyOpus/Objects", "FromFile": "assets/objects.png"},
            {"Action": "Include", "FromFile": "data/registries.json, data/items.json"},
        ])]), ["The base pack fills ValleyOpus's registries through the same door as every other pack (doc 09 §4a)."] + header)

    import build_art
    build_art.build(args.art_root, sprites, os.path.join(PACK, "assets", "objects.png"))
    print(f"Wrote {len(objects)} objects, {len(big)} big craftables, {len(registries['Recipes'])} recipes, {len(i18n)} strings.")


if __name__ == "__main__":
    main()
