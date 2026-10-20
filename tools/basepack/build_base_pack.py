"""Builds content/[CP] ValleyOpus from slice_data.py, full_data.py and world_data.py, plus its placeholder art.

Run from the repository root:  python3 tools/basepack/build_base_pack.py [--art-root /mnt/project-files/art-stockpile]

The pack is committed, so the game and CI never run this; rerun it after editing the data files. Base-pack rows
write the CharlesDR.ValleyOpus_ prefix literally: in this pack {{ModId}} is CharlesDR.ValleyOpus.CP (doc 12)."""

import argparse
import json
import os
from collections import OrderedDict

import slice_data as D
import full_data as F
import world_data as W

P = D.P
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PACK = os.path.join(ROOT, "content", "[CP] ValleyOpus")
TEXTURE = "Mods/CharlesDR.ValleyOpus/Objects"
# Data/Furniture is slash-delimited, so the texture path uses backslashes (SOS Pass B: "Failed loading texture Mods").
FURNITURE_TEXTURE = "Mods\\CharlesDR.ValleyOpus\\Furniture"
D_REAGENT = "valleyopus_reagent"

SORT_DARK, SORT_T3, SORT_WORN, SORT_NEW, SORT_CURIO = 2150, 9000, 3000, 3100, 3200


def q(local):
    return "(O)" + P + local


def spec(name):
    """An ingredient written in the data files: a local substance or Prime, (O)123 for a vanilla item, #tag."""
    if name.startswith("#"):
        return OrderedDict([("Tag", name[1:])])
    if name.startswith("("):
        return OrderedDict([("Item", name)])
    return OrderedDict([("Item", P + name)])


def build(art_root):
    i18n = OrderedDict()
    objects, big, crafting, trinkets, furniture = OrderedDict(), OrderedDict(), OrderedDict(), OrderedDict(), OrderedDict()
    registries = OrderedDict((name, OrderedDict()) for name in [
        "Primes", "Operations", "Conditions", "Apparatus", "Substances", "Recipes", "SecretPools",
        "Decomposition/TagWeights", "Decomposition/CategoryDefaults", "Decomposition/Overrides", "Decomposition/Settings",
        "Products", "Effects", "XpSources", "Codex/Pages", "Codex/Lore", "Milestones",
        "Unlocks", "WorkshopRooms", "WorkshopBundles", "LeyNodes", "Hints", "LabTiers", "Familiars", "Professions",
        "Contracts", "Npcs"])
    sprites = []  # (local, kind, colour/icon, rune, flask row)
    prime_colour = {p[0]: p[3] for p in D.PRIMES}
    used_runes = set()
    learned_by = OrderedDict()  # crafting recipe -> substance whose discovery teaches it (doc 14 §1)

    def rune_for(wanted):
        if wanted and wanted not in used_runes:
            used_runes.add(wanted)
            return wanted
        for r in range(5):
            for c in range(16):
                if (r, c) not in used_runes:
                    used_runes.add((r, c))
                    return (r, c)
        return (0, 0)

    for p in D.PRIMES:
        used_runes.add(p[5])

    def add_sprite(local, kind, value=None, rune=None, flask_row=None):
        sprites.append((local, kind, value, rune, flask_row))
        return len(sprites) - 1

    def add_object(local, display, gloss, sort, category, price, tags, colour, rune, flask_row, extra=None, name=None):
        index = add_sprite(local, "flask", colour, rune, flask_row)
        i18n[f"item.{local}.name"] = display
        i18n[f"item.{local}.desc"] = gloss
        row = OrderedDict([
            ("Name", name or f"{P}{sort}_{local}"),
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

    def add_icon_object(local, display, desc, sort, category, price, icon, tags, extra=None, obj_type="Basic"):
        index = add_sprite(local, "icon", icon)
        i18n[f"item.{local}.name"] = display
        i18n[f"item.{local}.desc"] = desc
        row = OrderedDict([
            ("Name", f"{P}{sort}_{local}" if sort else P + local),
            ("DisplayName", "{{i18n:item." + local + ".name}}"),
            ("Description", "{{i18n:item." + local + ".desc}}"),
            ("Type", obj_type), ("Category", category), ("Price", price),
            ("Texture", TEXTURE), ("SpriteIndex", index), ("Edibility", -300), ("ContextTags", tags)])
        if extra:
            row.update(extra)
        objects[P + local] = row
        return index

    # ── Primes ──
    for local, display, group, colour, order, rune, price, gloss in D.PRIMES:
        registries["Primes"][P + local] = OrderedDict([
            ("ItemId", q(local)), ("Group", group), ("Color", colour), ("SortOrder", order),
            ("NameKey", "item." + local + ".name"), ("GlossKey", "item." + local + ".desc")])
        add_object(local, display, gloss, D.SORT_PRIME + order, -16, price,
                   [D_REAGENT, "valleyopus_prime", "valleyopus_group_" + group.lower()], colour, rune, 1)

    # ── Operations ──
    for op in D.OPERATIONS + ["Ritual"]:
        registries["Operations"][P + op] = {"NameKey": "operation." + op}
        i18n["operation." + op] = op

    # ── Conditions ──
    registries["Conditions"][P + "Flowers"] = OrderedDict([
        ("Kind", "Adjacency"), ("Adjacency", {"Tag": "flower_item", "Directions": "All", "Range": 2}),
        ("NameKey", "condition.Flowers"), ("HintKey", "condition.Flowers.hint")])
    i18n["condition.Flowers"] = "Flowers nearby"
    i18n["condition.Flowers.hint"] = "Something here likes the smell of flowers."
    for cid, c in F.CONDITIONS.items():
        row = OrderedDict((k, v) for k, v in c.items() if k not in ("name", "hint"))
        row["NameKey"] = "condition." + cid
        row["HintKey"] = "condition." + cid + ".hint"
        registries["Conditions"][P + cid] = row
        i18n["condition." + cid] = c["name"]
        i18n["condition." + cid + ".hint"] = c["hint"]

    # ── Apparatus ──
    for local, display, kind, sprite, states, ops, desc in F.APPARATUS:
        row = OrderedDict([("ItemId", "(BC)" + P + local), ("Kind", kind)])
        if local == "ExperimentTable":
            row.update([("NeighbourStates", [{"Id": "Table", "Test": {}}]),
                        ("Operations", [{"Operation": P + op, "States": ["Table"]} for op in D.OPERATIONS]),
                        ("TrayCapacity", 2), ("Trigger", "Hand"), ("Automation", "None"), ("Chaining", False)])
        else:
            row["NeighbourStates"] = [{"Id": s, "Test": F.STATES[s]} for s in states]
            row["Operations"] = [{"Operation": P + op, "States": st} for op, st in ops.items()]
        row.update(F.APPARATUS_EXTRA.get(local, {}))
        row["FeedbackStyle"] = "Mechanical" if kind == "Engineered" else "Arcane"
        row["UnlockFlag"] = P + "Unlock_" + W.APPARATUS_UNLOCK.get(local, "Intro")
        row["NameKey"] = "apparatus." + local + ".name"
        registries["Apparatus"][P + local] = row
        i18n["apparatus." + local + ".name"] = display
        i18n["apparatus." + local + ".desc"] = desc
        big[P + local] = OrderedDict([
            ("Name", P + local), ("DisplayName", "{{i18n:apparatus." + local + ".name}}"),
            ("Description", "{{i18n:apparatus." + local + ".desc}}"), ("Price", 100), ("Fragility", 0),
            ("CanBePlacedOutdoors", True), ("CanBePlacedIndoors", True), ("IsLamp", local in ("Athanor", "Boiler")),
            ("Texture", "TileSheets/Craftables"), ("SpriteIndex", sprite),
            ("ContextTags", ["valleyopus_apparatus", "valleyopus_apparatus_" + local.lower()])])
    clocal, cdisplay, ckind, cops = F.CIRCLE
    registries["Apparatus"][P + clocal] = OrderedDict([
        ("Kind", ckind), ("NeighbourStates", [{"Id": "Open", "Test": {}}]),
        ("Operations", [{"Operation": P + op, "States": st} for op, st in cops.items()]),
        ("TrayCapacity", 4), ("Trigger", "Hand"), ("Automation", "StageOnly"), ("Chaining", False), ("QualityCap", 4),
        ("FeedbackStyle", "Arcane"), ("UnlockFlag", P + "Unlock_CrystalVault"), ("NameKey", "apparatus." + clocal + ".name")])
    i18n["apparatus." + clocal + ".name"] = cdisplay

    # ── Recipes ──
    def places_for(op, marks, runners=None):
        places = []
        if "A" in marks:
            places += F.OP_RUNNERS[op]["arc"]
        if "E" in marks:
            places += F.OP_RUNNERS[op]["eng"]
        if "R" in marks:
            places.append(("RitualAltar", "Open"))
        if "C" in marks or "R" in marks and op == "Ritual":
            places.append(("RitualCircle", "Open"))
        if runners:
            places += runners
        return places

    def recipe_row(local, inputs, op, marks, tier, conditions=(), runners=None, output=None, hand=None, requires=None,
                   quality=None, minutes=None):
        counts = OrderedDict()
        for i in inputs:
            counts[i] = counts.get(i, 0) + 1
        places = places_for(op, marks, runners)
        table = "T" in marks or "3" in marks
        ids, states = [], []
        if table:
            ids.append(P + "ExperimentTable")
            states.append("Table")
        for a, s in places:
            if P + a not in ids:
                ids.append(P + a)
            if s not in states:
                states.append(s)
        conds = []
        if op in D.FLOWER_BONUS_OPS and "A" in marks:
            conds.append({"ConditionId": P + "Flowers", "Use": "Bonus", "Effect": {"QualitySteps": 1}})
        if op == "Calcination" and ("A" in marks or "E" in marks):
            conds.append({"ConditionId": P + "LabHearth", "Use": "Bonus", "Effect": {"SpeedMultiplier": 0.75}})
        for cid, use in conditions:
            conds.append(OrderedDict([("ConditionId", P + cid), ("Use", use)]))
        inputs_rows = []
        for k, n in counts.items():
            s = spec(k)
            if n > 1:
                s["Count"] = n
            inputs_rows.append(s)
        row = OrderedDict([
            ("Inputs", inputs_rows),
            ("Operation", P + op),
            ("ApparatusIds", ids),
            ("NeighbourStates", states),
            ("Conditions", conds),
            ("Output", {"Id": output or P + local}),
            ("Quality", quality or {"Mode": "InheritMin"}),
            ("Minutes", minutes or (120 if tier == 1 else 240 if tier == 2 else 480)),
        ])
        if hand if hand is not None else "H" in marks:
            row["HandMadeOnly"] = True
        if not table:
            row["Discovery"] = {"TableAttemptable": False, "HintKeys": ["hint.recipe." + local]}
        if requires:
            row["RequiresUnlock"] = P + requires
        return row

    def product_extra(local):
        prod = F.PRODUCTS.get(local) or D.PRODUCTS.get(local)
        if not prod:
            return None, -16, [D_REAGENT, "valleyopus_substance"]
        extra = OrderedDict()
        tags = ["valleyopus_product", "valleyopus_substance"]
        if prod["Family"] == "Reagent":
            tags.append(D_REAGENT)
        if "Edible" in prod:
            e = prod["Edible"]
            extra["Edibility"] = e["Edibility"]
            extra["IsDrink"] = e.get("IsDrink", False)
            if e.get("Effects"):
                extra["Buffs"] = [{"Id": P + eff, "BuffId": P + eff} for eff in e["Effects"]]
        if "Apply" in prod or "Field" in prod:
            tags.append("valleyopus_apply")
        if "Dye" in prod:
            tags += ["color_" + prod["Dye"], "dye_strong"]
        if prod.get("Metal"):
            tags.append("valleyopus_metal_bar")
        if prod.get("Fuel"):
            tags.append("valleyopus_fuel")
        if local in ("RosMaialis",):
            tags.append(D_REAGENT)  # also a Vigor input
        if prod["Family"] == "Gift":
            tags.append("valleyopus_gift")
        return extra, prod["Category"], tags

    def add_substance(local, display, inputs, op, cls, marks, rune, gloss, tier, sort, conditions=(), runners=None,
                      requires=None):
        sub = OrderedDict([("Class", cls), ("Tier", tier)])
        if cls == "Item":
            sub["ItemId"] = q(local)
            sub["Bottleable"] = True
        sub["NameKey"] = "item." + local + ".name"
        sub["GlossKey"] = "item." + local + ".desc"
        registries["Substances"][P + local] = sub
        if inputs:
            registries["Recipes"][P + local] = recipe_row(local, inputs, op, marks, tier, conditions, runners, requires=requires)
        if cls == "Item":
            known = [prime_colour[i] for i in inputs if i in prime_colour]
            colour = blend(known) if known else "#6E6A66"
            prime_colour.setdefault(local, colour)
            extra, category, tags = product_extra(local)
            tags = tags + ["valleyopus_tier_" + str(tier)]
            add_object(local, display, gloss, sort, category, 1, tags, colour, rune_for(rune), 1 if tier == 1 else 2, extra)
            registries["Decomposition/Overrides"][q(local)] = (
                {"Profile": profile_of(inputs), "Units": max(1, tier)} if inputs else {"Refuse": True})
        else:
            i18n[f"item.{local}.name"] = display
            i18n[f"item.{local}.desc"] = gloss
            i18n["substance." + local] = display

    tier1_inputs = {t[0]: t[2] for t in D.TIER1}
    all_inputs = dict(tier1_inputs)
    all_inputs.update({t[0]: t[2] for t in D.TIER2})
    all_inputs.update({t[0]: t[2] for t in F.TIER2})
    all_inputs.update({t[0]: [t[2]] for t in F.DARK_TWINS})
    all_inputs.update({t[0]: t[2] for t in F.TIER3})
    prime_ids = {p[0] for p in D.PRIMES}

    def flatten(name, depth=0):
        if name in prime_ids:
            return [name]
        if name in all_inputs and depth < 6:
            return [x for i in all_inputs[name] for x in flatten(i, depth + 1)]
        return ["Terra"]  # vanilla inputs and Caput Mortuum count as earth

    def profile_of(inputs):
        prof = OrderedDict()
        for i in (x for name in inputs for x in flatten(name)):
            prof[P + i] = prof.get(P + i, 0) + 1
        return prof

    for n, (local, display, inputs, op, cls, table_only, rune, gloss) in enumerate(D.TIER1):
        marks = F.TIER1_MARKS[local]
        runners = None
        if local in F.POWER_SOURCES:
            runners = [F.POWER_SOURCES[local]]
            marks = "T"
        add_substance(local, display, inputs, op, cls, marks, rune, gloss, 1, D.SORT_T1 + n, runners=runners)
    cm_local, cm_display, cm_gloss = D.CAPUT_MORTUUM
    add_substance(cm_local, cm_display, [], None, "Item", "", (4, 14), cm_gloss, 1, D.SORT_RESIDUE)
    slice_t2_marks = {"Vitrum": "TEA", "TerraViva": "TEA", "Vigor": "TEA", "Smaltum": "TEA"}
    for n, (local, display, inputs, op, rune, gloss) in enumerate(D.TIER2):
        add_substance(local, display, inputs, op, "Item", slice_t2_marks[local], rune, gloss, 2, D.SORT_T2 + n)
    for n, (local, display, inputs, op, marks, conds, extra, rune, gloss) in enumerate(F.TIER2):
        add_substance(local, display, inputs, op, "Item", marks, rune, gloss, 2, D.SORT_T2 + len(D.TIER2) + n,
                      conditions=conds, runners=extra.get("Runners"))
    for n, (local, display, source, rune, gloss) in enumerate(F.DARK_TWINS):
        add_substance(local, display, [source], "Fermentation", "Item", "AR", rune, gloss, 2, SORT_DARK + n,
                      conditions=[("Darkness", "Required")])
        registries["Recipes"][P + local]["Conditions"].append(
            {"ConditionId": P + "Shades", "Use": "Bonus", "Effect": {"QualitySteps": 1}})
        registries["Recipes"][P + local]["TwinGroup"] = P + source
    for n, (local, display, inputs, op, marks, rune, gloss) in enumerate(F.TIER3):
        add_substance(local, display, inputs, op, "Item", marks + "H", rune, gloss, 3, SORT_T3 + n, requires="GreatWork")
        registries["Recipes"][P + local]["Discovery"] = {"TableAttemptable": False, "HintKeys": ["hint.recipe." + local]}

    # Stage advances: the input comes back one quality step higher (doc 12 §5).
    for rid, apparatus, op, catalyst, cap in F.STAGE_ADVANCES:
        registries["Recipes"][P + rid] = OrderedDict([
            ("Inputs", [{"Tag": "valleyopus_substance"}, {"Item": P + catalyst}]),
            ("Operation", P + op), ("ApparatusIds", [P + apparatus]), ("NeighbourStates", ["Open"]),
            ("Conditions", []), ("Output", {"Id": F.SAME_AS_INPUT}),
            ("Quality", {"Mode": "Advance", "Steps": 1, "Cap": cap}), ("Minutes", 360),
            ("Discovery", {"TableAttemptable": False, "HintKeys": ["hint.recipe." + rid]})])
        i18n["hint.recipe." + rid] = f"Circulate any substance with {catalyst} in the {apparatus}, and it comes back finer."

    # The secret recipe pool (doc 07 §6, doc 12 §11): one of these exists on each farm.
    registries["SecretPools"][P + "FarmSecret"] = {"PickCount": 1}
    for n, (sid, display, inputs, op, gloss) in enumerate(W.SECRET_CANDIDATES):
        add_substance(sid, display, inputs, op, "Item", "R", None, gloss, 3, SORT_T3 + 50 + n)
        registries["Recipes"][P + sid]["SecretPool"] = P + "FarmSecret"
        registries["Recipes"][P + sid]["HandMadeOnly"] = True
        registries["Products"][P + sid] = OrderedDict([("Family", "Sellable"), ("Uses", []),
                                                       ("Price", {"Mode": "Fixed", "Value": 3000}), ("CategoryRole", "Artisan")])

    for rid, row in registries["Recipes"].items():
        local = rid[len(P):]
        i18n.setdefault("hint.recipe." + local, W.recipe_hint(local, row))

    # ── Tools ──
    for local, display, desc, kind in [
            ("Satchel", "Reagent Satchel", "Holds reagents in its own pages. Primes and inputs you pick up go straight in. Click an apparatus while holding it to load a recipe.", "satchel"),
            ("Lens", "Assayer's Lens", "Hold it and click an item, a chest or a machine to see what it is made of.", "lens")]:
        index = add_sprite(local, kind)
        i18n[f"item.{local}.name"] = display
        i18n[f"item.{local}.desc"] = desc
        objects[P + local] = OrderedDict([
            ("Name", P + local), ("DisplayName", "{{i18n:item." + local + ".name}}"),
            ("Description", "{{i18n:item." + local + ".desc}}"), ("Type", "Basic"), ("Category", 0), ("Price", 0),
            ("Texture", TEXTURE), ("SpriteIndex", index), ("Edibility", -300), ("ExcludeFromShippingCollection", True),
            ("ExcludeFromFishingCollection", True), ("ExcludeFromRandomSale", True),
            ("ContextTags", ["valleyopus_tool", "not_giftable", "not_placeable"])])
    lid, ldisplay, licon, ldesc = F.GLOAM_LANTERN
    add_icon_object(lid, ldisplay, ldesc, None, 0, 0, licon, ["valleyopus_tool", "not_giftable", "not_placeable"],
                    {"ExcludeFromShippingCollection": True, "ExcludeFromRandomSale": True})

    # ── Worn items: trinkets (Data/Trinkets) and rings (Data/Objects Type Ring) ──
    for n, (local, display, kind, effect, icon, price, desc) in enumerate(F.WORN):
        if kind == "Ring":
            add_icon_object(local, display, desc, SORT_WORN + n, -96, price, icon,
                            ["valleyopus_ring", "valleyopus_effect_" + effect], obj_type="Ring")
            yield_id = q(local)
        else:
            index = add_sprite(local, "icon", icon)
            i18n[f"item.{local}.name"] = display
            i18n[f"item.{local}.desc"] = desc
            trinkets[P + local] = OrderedDict([
                ("Id", P + local), ("DisplayName", "{{i18n:item." + local + ".name}}"),
                ("Description", "{{i18n:item." + local + ".desc}}"), ("Texture", TEXTURE), ("SheetIndex", index),
                ("TrinketEffectClass", "ValleyOpus.Effects.AlchemyTrinket, ValleyOpus"), ("DropsNaturally", False),
                ("CanBeReforged", False), ("CustomFields", {"CharlesDR.ValleyOpus/Effect": P + effect})])
            yield_id = "(TR)" + P + local
        ingredients, learn = F.WORN_CRAFTING[local]
        crafting[P + local] = f"{ingredients}/Home/{yield_id}/false/none/"
        learned_by[P + local] = P + learn

    # ── New produce items, curios, placed helpers ──
    for n, (local, display, icon, price, category, desc) in enumerate(F.NEW_ITEMS):
        add_icon_object(local, display, desc, SORT_NEW + n, category, price, icon, ["valleyopus_produce"])
    for n, (local, display, icon, lore) in enumerate(F.CURIOS):
        add_icon_object(local, display, lore, SORT_CURIO + n, 0, 0, icon, ["valleyopus_curio", "not_giftable"],
                        {"ExcludeFromShippingCollection": True, "ExcludeFromRandomSale": True})
    for local, display, sprite, ingredients, learn, desc in F.PLACED:
        i18n["apparatus." + local + ".name"] = display
        i18n["apparatus." + local + ".desc"] = desc
        big[P + local] = OrderedDict([
            ("Name", P + local), ("DisplayName", "{{i18n:apparatus." + local + ".name}}"),
            ("Description", "{{i18n:apparatus." + local + ".desc}}"), ("Price", 500), ("Fragility", 0),
            ("CanBePlacedOutdoors", True), ("CanBePlacedIndoors", True), ("Texture", "TileSheets/Craftables"),
            ("SpriteIndex", sprite), ("ContextTags", ["valleyopus_placed", "valleyopus_placed_" + local.lower()])])
        if ingredients:
            crafting[P + local] = f"{ingredients}/Home/{P}{local}/true/none/"
            learned_by[P + local] = P + learn
    # The Radicula ritual makes the placeable plant directly.
    registries["Recipes"][P + "Radicula"]["Output"] = {"Id": "(BC)" + P + "RadiculaPlant"}

    # ── Furniture (Circle Sigil, glazed décor) ──
    import build_art
    furniture_cols, _, _ = build_art.furniture_layout(F.FURNITURE)
    for n, (local, display, ftype, size, _, desc) in enumerate(F.FURNITURE):
        w, h = size
        i18n[f"furniture.{local}.name"] = display
        i18n[f"furniture.{local}.desc"] = desc
        bounds = f"{w} {h}" if ftype == "rug" else f"{w} 1"
        furniture[P + local] = f"{P}{local}/{ftype}/{w} {h}/{bounds}/1/500/-1/{{{{i18n:furniture.{local}.name}}}}/{furniture_cols[n]}/{FURNITURE_TEXTURE}/true/valleyopus_furniture"

    # ── Alternate vanilla crafting recipes and décor (doc 15 §8, §9) ──
    for rid, (ingredients, yield_id, count, learn) in F.ALT_CRAFTING.items():
        crafting[P + rid] = f"{ingredients}/Home/{yield_id} {count}/false/none/"
        learned_by[P + rid] = P + learn

    # ── Decomposition tables ──
    for cat, weights in D.CATEGORY_DEFAULTS.items():
        registries["Decomposition/CategoryDefaults"][cat] = {"Weights": {P + k: v for k, v in weights.items()}}
    for tag, weights in list(D.TAG_WEIGHTS.items()) + list(F.MORE_TAG_WEIGHTS.items()):
        registries["Decomposition/TagWeights"][tag] = {"Weights": {P + k: v for k, v in weights.items()}}
    for tag, weights in list(D.AETHER_SOURCES.items()) + list(F.MORE_AETHER_SOURCES.items()):
        registries["Decomposition/TagWeights"][tag] = {"Weights": {P + k: v for k, v in weights.items()}, "AetherSource": True}
    for local in ["CaputMortuum"] + [w[0] for w in F.NEW_ITEMS if w[0] != "BrimstoneEgg"] + [c[0] for c in F.CURIOS]:
        registries["Decomposition/Overrides"][q(local)] = {"Refuse": True}
    registries["Decomposition/Overrides"][q("BrimstoneEgg")] = {"Profile": {P + "Sulphur": 3, P + "Ignis": 1}, "Units": 4}
    registries["Decomposition/Settings"]["Default"] = OrderedDict([
        ("K", 1.0), ("E", 0.5), ("QualityBonus", 0.1), ("Threshold", 0.10), ("ExchangeRatio", 0.8),
        ("DefaultResidue", {"Count": 1, "Chance": 0.25}), ("ResidueItem", q("CaputMortuum"))])

    # ── Products and effects ──
    all_products = OrderedDict(D.PRODUCTS)
    all_products.update(F.PRODUCTS)
    for local, prod in all_products.items():
        uses = []
        if "Edible" in prod:
            e = prod["Edible"]
            uses.append(OrderedDict([("Kind", "Edible"), ("Energy", round(e["Edibility"] * 2.5)),
                                     ("Health", round(e["Edibility"] * 1.125)), ("IsDrink", e.get("IsDrink", False)),
                                     ("Effects", [P + x for x in e.get("Effects", [])])]))
        if "Apply" in prod:
            uses.append(OrderedDict([("Kind", "Apply"), ("Effects", [P + x for x in prod["Apply"]["Effects"]])]))
        if "Field" in prod:
            uses.append(OrderedDict([("Kind", "Apply"), ("System", "Field"), ("Ref", prod["Field"]["Handler"])]))
        if "Fertilizer" in prod:
            uses.append(OrderedDict([("Kind", "Fertilizer")] + list(prod["Fertilizer"].items())))
        if "Dye" in prod:
            uses.append({"Kind": "Material", "System": "Dye", "Ref": prod["Dye"]})
        if prod.get("Fuel"):
            uses.append({"Kind": "Material", "System": "Fuel", "Ref": "Calcinator"})
        if prod["Family"] == "Gift":
            uses.append({"Kind": "Gift"})
        price = {"Mode": "Fixed", "Value": prod["FixedPrice"]} if "FixedPrice" in prod else {"Mode": "Derived"}
        role = {-26: "Artisan", -19: "Fertilizer", -15: "Metal"}.get(prod["Category"], "Reagent")
        row = OrderedDict([("Family", prod["Family"]), ("Uses", uses), ("Price", price), ("CategoryRole", role)])
        registries["Products"][P + local] = row
    registries["Effects"][P + "UndinesGift"] = OrderedDict([
        ("Kind", "Custom"), ("Handler", "UndinesGift"), ("Params", {"Chance": "0.5"}), ("Duration", 240), ("Tier", "Weak"),
        ("NameKey", "effect.UndinesGift.name"), ("DescKey", "effect.UndinesGift.desc")])
    i18n["effect.UndinesGift.name"] = "Undine's Gift"
    i18n["effect.UndinesGift.desc"] = "Half of your waterings use no water."
    for eid, (kind, payload, params, duration, tier, name, desc) in F.EFFECTS.items():
        row = OrderedDict([("Kind", "Custom" if kind == "Custom" else "VanillaStats")])
        if kind == "Custom":
            row["Handler"] = payload
            row["Params"] = params
        else:
            row["Stats"] = payload
        row.update([("Duration", duration), ("Tier", tier), ("NameKey", "effect." + eid + ".name"), ("DescKey", "effect." + eid + ".desc")])
        registries["Effects"][P + eid] = row
        i18n["effect." + eid + ".name"] = name
        i18n["effect." + eid + ".desc"] = desc

    # ── XP sources (doc 05) ──
    for key, row in [
            ("Discovery", {"Event": "Discovery", "Amount": 100, "TierScale": True, "OncePer": "Recipe"}),
            ("EffectDiscovery", {"Event": "EffectDiscovery", "Amount": 25, "OncePer": "Effect"}),
            ("HandCollect", {"Event": "HandCollect", "Amount": 2, "CapPerAction": 20}),
            ("AutoCollect", {"Event": "AutoCollect", "Amount": 2, "CapPerAction": 20, "Scale": 0.25}),
            ("Assay", {"Event": "Assay", "Amount": 5, "OncePer": "Item"}),
            ("TriedKey", {"Event": "TriedKey", "Amount": 3, "OncePer": "Key"}),
            ("RitualTrigger", {"Event": "RitualTrigger", "Amount": 15})]:
        registries["XpSources"][P + key] = row

    # ── Codex pages, lore and milestones (doc 07, doc 16 §6) ──
    for order, (key, title) in enumerate([("Primes", "The Primes"), ("Tier1", "The First Work"), ("Tier2", "The Second Work"),
                                          ("Tier3", "The Great Work"), ("Apparatus", "Apparatus"), ("Unlocks", "The Workshop"),
                                          ("Notes", "Notes")]):
        registries["Codex/Pages"][P + key] = {"Group": key, "SortOrder": order * 10, "TitleKey": "codex.page." + key}
        i18n["codex.page." + key] = title
    for local, *_ in D.PRIMES:
        registries["Codex/Lore"][P + local + "_1"] = {"TargetId": q(local), "Threshold": 1, "TextKey": "lore." + local + ".1"}
    for k, v in W.PRIME_LORE.items():
        i18n["lore." + k + ".1"] = v
    for local, threshold, text in W.MORE_LORE:
        registries["Codex/Lore"][P + local + "_" + str(threshold)] = {"TargetId": q(local), "Threshold": threshold, "TextKey": f"lore.{local}.{threshold}"}
        i18n[f"lore.{local}.{threshold}"] = text
    for key, trigger, celebration, text in W.MILESTONES:
        registries["Milestones"][P + key] = {"Trigger": trigger, "Celebration": celebration, "TextKey": "milestone." + key}
        i18n["milestone." + key] = text

    # ── Apparatus and tool crafting recipes ──
    for local, (ingredients, kind) in F.CRAFTING.items():
        if kind == "(F)":
            crafting[P + local] = f"{ingredients}/Home/(F){P}{local}/false/none/"
        else:
            crafting[P + local] = f"{ingredients}/Home/{P}{local}/{'true' if kind else 'false'}/none/"

    # ── World: unlocks, rooms, bundles, ley nodes, Labs, Familiars, professions, hints, contracts, NPCs ──
    W.fill(registries, i18n, learned_by)

    return registries, objects, big, crafting, trinkets, furniture, i18n, sprites


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

    registries, objects, big, crafting, trinkets, furniture, i18n, sprites = build(args.art_root)
    header = ["Generated by tools/basepack/build_base_pack.py from slice_data.py, full_data.py and world_data.py; edit those and rerun.",
              "Base-pack rows write the CharlesDR.ValleyOpus_ prefix literally (doc 12): {{ModId}} here is the CP pack's ID."]

    write_json(os.path.join(PACK, "data", "registries.json"), {"Changes": [
        {"Action": "EditData", "Target": "CharlesDR.ValleyOpus/" + name, "Entries": rows}
        for name, rows in registries.items() if rows]}, header)
    write_json(os.path.join(PACK, "data", "items.json"), {"Changes": [
        {"Action": "EditData", "Target": "Data/Objects", "Entries": objects},
        {"Action": "EditData", "Target": "Data/BigCraftables", "Entries": big},
        {"Action": "EditData", "Target": "Data/CraftingRecipes", "Entries": crafting},
        {"Action": "EditData", "Target": "Data/Trinkets", "Entries": trinkets},
        {"Action": "EditData", "Target": "Data/Furniture", "Entries": furniture},
    ]}, header)
    import world_pack
    world_files = world_pack.write(PACK, i18n, write_json, header, args.art_root)
    write_json(os.path.join(PACK, "i18n", "default.json"), i18n)
    # The C# mod reads row names (NameKey, TextKey...) through this asset, so they translate with the pack.
    write_json(os.path.join(PACK, "data", "strings.json"), {"Changes": [
        {"Action": "EditData", "Target": "CharlesDR.ValleyOpus/Strings",
         "Entries": OrderedDict((k, "{{i18n:" + k + "}}") for k in i18n if not k.startswith("item."))}]}, header)
    write_json(os.path.join(PACK, "content.json"), OrderedDict([
        ("Format", "2.4.0"),
        ("Changes", [
            {"Action": "Load", "Target": "Mods/CharlesDR.ValleyOpus/Objects", "FromFile": "assets/objects.png"},
            {"Action": "Load", "Target": "Mods/CharlesDR.ValleyOpus/Furniture", "FromFile": "assets/furniture.png"},
            {"Action": "Include", "FromFile": ", ".join(["data/registries.json", "data/items.json", "data/strings.json"] + world_files)},
        ])]), ["The base pack fills ValleyOpus's registries through the same door as every other pack (doc 09 §4a)."] + header)

    import build_art
    build_art.build(args.art_root, sprites, os.path.join(PACK, "assets", "objects.png"))
    build_art.build_furniture(args.art_root, F.FURNITURE, os.path.join(PACK, "assets", "furniture.png"))
    print(f"Wrote {len(objects)} objects, {len(big)} big craftables, {len(registries['Recipes'])} recipes, "
          f"{len(trinkets)} trinkets, {len(furniture)} furniture, {len(i18n)} strings.")


if __name__ == "__main__":
    main()
