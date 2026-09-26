using System;
using System.Collections.Generic;
using System.Linq;

namespace Wordgrove;

/// <summary>
/// Embedded word lists organised by theme (50-100 words each)
/// plus a general bonus-word validation list (~2000 common 3-5 letter words).
/// All words stored UPPER-CASE.
/// </summary>
public static class WordDatabase
{
    // ── Theme catalogue (10+ themes) ───────────────────────────────
    public static readonly string[] ThemeNames =
    {
        "ANIMALS", "FOOD", "COLORS", "NATURE", "SPORTS",
        "MUSIC", "TRAVEL", "WEATHER", "FRUIT", "BODY",
        "OCEAN", "SPACE"
    };

    public static readonly Dictionary<string, string[]> ThemeWords = new()
    {
        ["ANIMALS"] = new[]
        {
            "CAT","DOG","BIRD","FISH","BEAR","DEER","DUCK","FROG","GOAT","HAWK",
            "LAMB","LION","MOLE","MOTH","NEWT","PONY","SEAL","SLUG","SWAN","TOAD",
            "WOLF","WORM","BULL","CALF","CLAM","COLT","CRAB","CROW","DOVE","FAWN",
            "GULL","HARE","HERD","LARK","LYNX","MARE","MICE","MINK","MULE","ORCA",
            "PUMA","RAVEN","ROBIN","SHARK","SNAKE","SQUID","STORK","TIGER","WHALE","ZEBRA",
            "MOUSE","HORSE","EAGLE","CRANE","FINCH","GECKO","HIPPO","HYENA","KOALA","LEMUR",
            "LLAMA","MOOSE","OTTER","PANDA","QUAIL","SKUNK","SLOTH","SNAIL","STAG","TROUT",
            "VIPER","CHIMP","BISON","CAMEL","COBRA","DINGO","DRAKE","EGRET","HERON","IBIS"
        },
        ["FOOD"] = new[]
        {
            "RICE","SOUP","STEW","CAKE","CORN","PORK","BEEF","LAMB","FISH","TACO",
            "WRAP","BAKE","BOWL","CHOP","COOK","DICE","DINE","DISH","FORK","GRILL",
            "HERB","LOAF","MEAL","MENU","MILK","MISO","NAAN","OATS","PEEL","ROLL",
            "SALT","STIR","TOAST","WHEAT","CREAM","GRAVY","PASTA","PIZZA","SALAD","SAUCE",
            "BREAD","BAGEL","BROTH","CHIPS","CRUST","CURRY","FLOUR","FRUIT","GRAIN","HONEY",
            "JUICE","LEMON","MANGO","MAPLE","OLIVE","ONION","PEACH","SPICE","SUGAR","SYRUP",
            "BACON","CANDY","ROAST","SWEET","BLEND","BREWS","FEAST","LUNCH","SNACK","TASTE",
            "BASIL","CHILI","CLOVE","CUMIN","DOUGH","GLAZE","MELON","PECAN","PLUM","PRAWN"
        },
        ["COLORS"] = new[]
        {
            "RED","TAN","ASH","DIM","HUE","DYE","INK","SKY","JET","OAK",
            "BLUE","CYAN","GOLD","GRAY","JADE","LIME","MINT","NAVY","PINK","PLUM",
            "ROSE","RUBY","RUST","SAGE","SAND","TEAL","WINE","AMBER","BLACK","BLUSH",
            "BROWN","COCOA","CORAL","CREAM","EBONY","GREEN","IVORY","KHAKI","LEMON","LILAC",
            "MAUVE","OLIVE","PEACH","SLATE","WHITE","BEIGE","BRICK","CEDAR","CHALK","CHARM",
            "DUSTY","FADED","FLUSH","FROST","LIGHT","MISTY","MOCHA","PAINT","PEARL","SHADE",
            "SHINE","SMOKE","STEEL","STONE","SUNNY","VIVID","BLAZE","FLAME","GLEAM","GLOW",
            "PASTEL","SCARLET","CRIMSON","VIOLET","INDIGO","SIENNA","UMBER","TAUPE","AZURE","OCHRE"
        },
        ["NATURE"] = new[]
        {
            "TREE","LEAF","MOSS","FERN","VINE","BARK","ROOT","SEED","BUD","SAP",
            "HILL","LAKE","POND","ROCK","SAND","SOIL","CAVE","CLAY","DALE","DELL",
            "GLEN","GULF","ISLE","MESA","MOOR","PEAK","RIFT","VALE","BLOOM","CLIFF",
            "CREEK","FIELD","FLORA","GROVE","HEDGE","MARSH","PLAIN","RIDGE","SHORE","SLOPE",
            "STONE","STORM","SWAMP","TRAIL","WOODS","BROOK","FALLS","GLADE","HAVEN","INLET",
            "OASIS","OCEAN","RIVER","WATER","EARTH","GRASS","PLANT","PETAL","THORN","CORAL",
            "DELTA","DRIFT","FROST","GORGE","KNOLL","LEDGE","MOUNT","RANGE","REALM","HEATH",
            "COAST","BASIN","BLUFF","DUNES","FJORD","GULCH","MARSH","PINES","REEDS","STREAM"
        },
        ["SPORTS"] = new[]
        {
            "RUN","HIT","WIN","AIM","BAT","BOW","CUP","FAN","GYM","JAB",
            "BALL","BOWL","CLUB","DART","DIVE","DUNK","FAST","FOUL","GOAL","GRIP",
            "HALF","JUMP","KICK","LAPS","LOSS","NETS","PACE","PASS","PLAY","RACE",
            "SHOT","SLAM","SPIN","TEAM","TOSS","WALK","YARD","BLOCK","CATCH","CHASE",
            "COACH","COURT","DRAFT","DRIVE","FIELD","FINAL","GUARD","MATCH","MEDAL","PITCH",
            "POINT","POWER","PUNCH","RALLY","RELAY","ROUND","SCORE","SERVE","SKILL","SPEED",
            "SPORT","SQUAD","STAKE","SWEEP","SWING","THROW","TRACK","TRAIN","TRIAL","VAULT",
            "BENCH","BRAWL","CLASH","DARTS","FENCE","KAYAK","REIGN","RIDER","SKATE","SURGE"
        },
        ["MUSIC"] = new[]
        {
            "BAR","BOP","DUO","GIG","HIT","HUM","JAM","KEY","MIX","POP",
            "BAND","BASS","BEAT","BELL","CLAP","DRUM","DUET","ECHO","FLAT","FOLK",
            "FRET","GONG","HARP","HORN","HYMN","JAZZ","KEYS","LUTE","LYRA","MUTE",
            "NOTE","OBOE","PICK","PIPE","REST","RIFF","RING","SING","SNAP","SOLO",
            "SONG","TONE","TRIO","TUBA","TUNE","VIBE","ALTO","CHORD","CHOIR","FLUTE",
            "FORTE","GENRE","LYRIC","MAJOR","METER","MINOR","MUSIC","OPERA","ORGAN","PIANO",
            "PITCH","PULSE","REMIX","RHYTHM","SCALE","SHARP","SNARE","SOUND","STRUM","SWING",
            "TEMPO","TENOR","VIOLA","VOCAL","WALTZ","BANJO","BRASS","CELLO","CHIME","CLANG"
        },
        ["TRAVEL"] = new[]
        {
            "BUS","CAB","CAR","FLY","INN","JET","MAP","RUN","TIP","VAN",
            "BOAT","CAMP","CITY","DOCK","EAST","FARE","GATE","HIKE","LAKE","LANE",
            "MILE","PACK","PATH","PORT","RIDE","ROAD","SAIL","SHIP","STOP","TAXI",
            "TOUR","TOWN","TREK","TRIP","VISA","WEST","VISIT","BEACH","BOARD","CABIN",
            "COAST","DRIVE","FERRY","GUIDE","HOTEL","LODGE","NORTH","PILOT","PLANE","ROUTE",
            "SOUTH","STEAM","SUITE","TRAIN","WHARF","ATLAS","BADGE","BERTH","CHART","DEPART",
            "DRIFT","GLOBE","HAVEN","KAYAK","OASIS","QUEST","TRAIL","YACHT","MOTEL","RIDER",
            "CANOE","FLEET","LINER","METRO","RAILS","BARGE","CARGO","CRUISE","DRIVE","ROAM"
        },
        ["WEATHER"] = new[]
        {
            "AIR","DEW","DRY","FOG","HOT","ICE","SUN","WET","SKY","DAY",
            "BLOW","CALM","COLD","COOL","DAMP","DUSK","GALE","GUST","HAIL","HAZE",
            "HEAT","MELT","MILD","MIST","RAIN","SLID","SMOG","SNOW","THAW","WARM",
            "WIND","BOLT","DAWN","FADE","FALL","HIGH","PALE","PURE","RISE","VENT",
            "BLAST","BLAZE","CLOUD","DRAFT","FLOOD","FROST","HUMID","LIGHT","MUGGY","POLAR",
            "SLEET","SOLAR","STORM","SUNNY","SURGE","VAPOR","BRISK","CHILL","CLEAR","DRIZZLE",
            "DUSTY","FLAKE","FRONT","MISTY","RAINY","SHADE","SHINE","WINDY","DEWY","FLASH",
            "LUNAR","BALMY","BREEZY","CLOUDY","DENSE","FOGGY","SNOWY","TEPID","TIDAL","TORRID"
        },
        ["FRUIT"] = new[]
        {
            "FIG","NUT","YAM","JAM","PIE","DIP","MIX","CUP","JAR","PIT",
            "APPLE","BERRY","GRAPE","GUAVA","LEMON","MANGO","MELON","OLIVE","PEACH","PLUM",
            "PRUNE","CHERRY","CITRUS","BANANA","LIME","PEAR","DATE","KIWI","SEED","RIND",
            "PULP","TART","RIPE","ZEST","TANG","JUICY","SWEET","FRESH","FLESH","SLICE",
            "BLEND","DRIED","CRISP","SNACK","PUREE","SYRUP","JELLY","SAUCE","PASTE","JUICE",
            "STEM","CORE","SKIN","VINE","LEAF","CROP","PICK","TREE","GROVE","ORCHARD",
            "NECTAR","FIBER","WEDGE","CHUNK","SPEAR","HALVE","SCOOP","SQUASH","CRUSH","PRESS",
            "PITH","SOUR","ACID","ACAI","PALM","CANE","BEET","GOURD","BEAN","HERB"
        },
        ["BODY"] = new[]
        {
            "ARM","EAR","EYE","GUT","HIP","JAW","LEG","LIP","RIB","TOE",
            "BACK","BONE","CHIN","FACE","FOOT","HAIR","HAND","HEAD","HEEL","KNEE",
            "NAIL","NECK","NOSE","PALM","SHIN","SKIN","VEIN","WRIST","ANKLE","BRAIN",
            "CHEEK","CHEST","ELBOW","LIVER","SKULL","SPINE","TEETH","THIGH","THUMB","WAIST",
            "BROW","CORE","FIST","LUNG","CELL","LOBE","LIMB","PORE","ARCH","CALF",
            "IRIS","LASH","MANE","SCAR","SOLE","TORSO","TRUNK","PULSE","NERVE","JOINT",
            "FIBER","GLAND","HEART","MOUTH","ORGAN","FLESH","BLADE","CROWN","RIDGE","FRAME",
            "SHANK","SCALP","NAVEL","BICEP","FEMUR","TENDON","PELVIS","MARROW","MUSCLE","TISSUE"
        },
        ["OCEAN"] = new[]
        {
            "BAY","EEL","FIN","NET","OAR","SEA","BAR","BOW","DAM","DIP",
            "BOAT","BUOY","CLAM","COVE","CRAB","DEEP","DOCK","FISH","FOAM","GULL",
            "HULL","KELP","KEEL","KNOT","MAST","PIER","POOL","PORT","REEF","SAIL",
            "SAND","SHIP","SURF","TIDE","WAKE","WAVE","BEACH","COAST","CORAL","DRIFT",
            "FLEET","INLET","PEARL","PLANK","PRAWN","RIVET","SHARK","SHELL","SHORE","SQUID",
            "STERN","STORM","WHALE","WRECK","ABYSS","BRINE","CANAL","CLIFF","CORAL","CREEK",
            "DELTA","DEPTH","DIVER","FLOAT","HAVEN","JETTY","OCEAN","OTTER","SHOAL","TROUT",
            "ALGAE","BILGE","CABIN","CARGO","CHART","CRANE","FLEET","MORAY","PLUME","SPRAY"
        },
        ["SPACE"] = new[]
        {
            "ARC","GAS","ION","ORB","RAY","SKY","SUN","DIM","DOT","JET",
            "BEAM","BOLT","BURN","COMET","CORE","DARK","DAWN","DOME","DUST","EDGE",
            "FALL","FIRE","FUEL","GLOW","HALO","HULL","IRON","LENS","LIFT","LINK",
            "LOOP","MARS","MASS","MOON","NOVA","ORBIT","PATH","PEAK","POLE","RING",
            "ROCK","SCAN","SILO","STAR","TAIL","VOID","WARP","ZONE","BLAST","COMET",
            "CRAFT","DEPTH","EARTH","EJECT","FLARE","FORCE","GAMMA","LASER","LIGHT","LUNAR",
            "NEBULA","ORBIT","PHASE","PILOT","PLUME","PROBE","PULSE","QUARK","RADAR","RANGE",
            "SOLAR","SPACE","SURGE","TRAIL","VAPOR","VENUS","WIELD","FLINT","FORGE","TITAN"
        }
    };

    // ── Bonus word validation list: ~2000 common 3-5 letter English words ──
    // These are used to validate bonus words (words that aren't target words
    // but ARE real English words).
    private static HashSet<string> _bonusWords;

    public static HashSet<string> BonusWords
    {
        get
        {
            _bonusWords ??= BuildBonusWordSet();
            return _bonusWords;
        }
    }

    public static bool IsValidBonusWord(string word)
    {
        return BonusWords.Contains(word.ToUpperInvariant());
    }

    /// <summary>
    /// Returns all theme words as a set for quick lookup.
    /// </summary>
    public static HashSet<string> AllThemeWordsSet()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in ThemeWords)
            foreach (var w in kv.Value)
                set.Add(w.ToUpperInvariant());
        return set;
    }

    private static HashSet<string> BuildBonusWordSet()
    {
        // ~2000 common 3-5 letter English words for bonus validation
        var words = new[]
        {
            // 3-letter words (≈500)
            "ACE","ACT","ADD","AGE","AGO","AID","AIM","AIR","ALL","AND",
            "ANT","ANY","APE","ARC","ARE","ARK","ARM","ART","ASH","ASK",
            "ATE","AWE","AXE","BAD","BAG","BAN","BAR","BAT","BAY","BED",
            "BET","BIG","BIT","BOW","BOX","BOY","BUD","BUG","BUN","BUS",
            "BUT","BUY","CAB","CAN","CAP","CAR","CAT","COP","COT","COW",
            "CRY","CUB","CUP","CUR","CUT","DAB","DAD","DAM","DAY","DEN",
            "DEW","DID","DIG","DIM","DIP","DOC","DOG","DOT","DRY","DUB",
            "DUD","DUE","DUG","DUN","DUO","DYE","EAR","EAT","EEL","EGG",
            "ELF","ELK","ELM","EMU","END","ERA","EVE","EWE","EYE","FAN",
            "FAR","FAT","FAX","FED","FEW","FIG","FIN","FIT","FIX","FLY",
            "FOB","FOE","FOG","FOP","FOR","FOX","FRY","FUN","FUR","GAB",
            "GAG","GAP","GAS","GAY","GEL","GEM","GET","GIG","GIN","GNU",
            "GOB","GOD","GOT","GUM","GUN","GUT","GUY","GYM","HAD","HAM",
            "HAS","HAT","HAY","HEN","HER","HEW","HID","HIM","HIP","HIS",
            "HIT","HOB","HOG","HOP","HOT","HOW","HUB","HUE","HUG","HUM",
            "HUT","ICE","ICY","ILL","IMP","INK","INN","ION","IRE","IRK",
            "ITS","IVY","JAB","JAG","JAM","JAR","JAW","JAY","JET","JIG",
            "JOB","JOG","JOT","JOY","JUG","JUT","KEG","KEN","KEY","KID",
            "KIN","KIT","LAB","LAD","LAG","LAP","LAW","LAY","LED","LEG",
            "LET","LID","LIE","LIP","LIT","LOG","LOT","LOW","LUG","MAD",
            "MAN","MAP","MAR","MAT","MAW","MAX","MAY","MEN","MET","MID",
            "MIX","MOB","MOM","MOP","MOW","MUD","MUG","NAB","NAG","NAP",
            "NET","NEW","NIL","NIT","NOB","NOD","NOR","NOT","NOW","NUB",
            "NUN","NUT","OAK","OAR","OAT","ODD","ODE","OFF","OFT","OIL",
            "OLD","ONE","OPT","ORB","ORE","OUR","OUT","OWE","OWL","OWN",
            "PAD","PAL","PAN","PAP","PAT","PAW","PAY","PEA","PEG","PEN",
            "PEP","PER","PET","PEW","PIE","PIG","PIN","PIT","PLY","POD",
            "POP","POT","POW","PRY","PUB","PUG","PUN","PUP","PUS","PUT",
            "RAG","RAM","RAN","RAP","RAT","RAW","RAY","RED","REF","RIB",
            "RID","RIG","RIM","RIP","ROB","ROD","ROE","ROT","ROW","RUB",
            "RUG","RUM","RUN","RUT","RYE","SAC","SAD","SAG","SAP","SAT",
            "SAW","SAY","SEA","SET","SEW","SHE","SHY","SIN","SIP","SIR",
            "SIS","SIT","SIX","SKI","SKY","SLY","SOB","SOD","SON","SOP",
            "SOT","SOW","SOY","SPA","SPY","STY","SUB","SUM","SUN","SUP",
            "TAB","TAD","TAG","TAN","TAP","TAR","TAT","TAX","TEA","TEN",
            "THE","TIE","TIN","TIP","TOE","TON","TOO","TOP","TOT","TOW",
            "TOY","TRY","TUB","TUG","TWO","URN","USE","VAN","VAT","VET",
            "VIA","VIE","VOW","WAD","WAG","WAR","WAS","WAX","WAY","WEB",
            "WED","WET","WHO","WIG","WIN","WIT","WOE","WOK","WON","WOO",
            "WOW","YAK","YAM","YAP","YAW","YEA","YES","YET","YEW","YOU",
            "ZAP","ZEN","ZIP","ZIT","ZOO",

            // 4-letter words (≈800)
            "ABLE","ACHE","ACID","ACRE","AGED","AIDE","ALLY","ALSO","AMID","ARCH",
            "AREA","ARMY","AUTO","AVID","AXLE","BABE","BACK","BAIT","BAKE","BALD",
            "BALE","BALL","BAND","BANE","BANG","BANK","BARE","BARK","BARN","BASE",
            "BATH","BEAD","BEAK","BEAM","BEAN","BEAT","BEEN","BELL","BELT","BEND",
            "BEST","BIKE","BILL","BIND","BIRD","BITE","BLAH","BLOW","BLUR","BOAR",
            "BOAT","BODY","BOLD","BOLT","BOMB","BOND","BONE","BOOK","BOOM","BOOT",
            "BORE","BORN","BOSS","BOTH","BOUT","BRAG","BRED","BREW","BRIM","BULB",
            "BULK","BULL","BUMP","BURN","BURR","BUST","BUSY","BUZZ","CAFE","CAGE",
            "CAKE","CALF","CALL","CALM","CAME","CAMP","CANE","CAPE","CARD","CARE",
            "CART","CASE","CASH","CAST","CAVE","CELL","CHAT","CHEF","CHIN","CHIP",
            "CHOP","CITE","CITY","CLAD","CLAM","CLAN","CLAP","CLAW","CLAY","CLIP",
            "CLUE","COAL","COAT","CODE","COIL","COIN","COLD","COLT","COME","COOK",
            "COOL","COPE","COPY","CORD","CORE","CORK","CORN","COST","COZY","CRAM",
            "CREW","CROP","CROW","CUBE","CULT","CURB","CURE","CURL","CUTE","DAMP",
            "DARE","DARK","DARN","DART","DASH","DATA","DATE","DAWN","DEAD","DEAF",
            "DEAL","DEAR","DEBT","DECK","DEED","DEEM","DEEP","DEER","DELI","DEMO",
            "DENT","DENY","DESK","DIAL","DICE","DIET","DIME","DINE","DIRT","DISC",
            "DISH","DOCK","DOES","DOME","DONE","DOOM","DOOR","DOSE","DOWN","DOZE",
            "DRAB","DRAG","DRAW","DREW","DROP","DRUM","DUAL","DUDE","DUEL","DULL",
            "DUMB","DUMP","DUNE","DUNG","DUNK","DUSK","DUST","DUTY","DYER","EACH",
            "EARL","EARN","EASE","EAST","EASY","EDGE","EDIT","ELSE","EMIT","EPIC",
            "EVEN","EVER","EVIL","EXAM","EXEC","EXIL","EYED","FACE","FACT","FADE",
            "FAIL","FAIR","FAKE","FALL","FAME","FANG","FARE","FARM","FAST","FATE",
            "FAWN","FEAR","FEAT","FEED","FEEL","FELL","FELT","FERN","FEST","FILE",
            "FILL","FILM","FIND","FINE","FIRE","FIRM","FISH","FIST","FLAG","FLAK",
            "FLAT","FLAW","FLEA","FLED","FLEW","FLIP","FLIT","FLOG","FLOW","FLUX",
            "FOAM","FOIL","FOLD","FOLK","FOND","FONT","FOOD","FOOL","FOOT","FORD",
            "FORE","FORK","FORM","FORT","FOUL","FOUR","FOWL","FREE","FROM","FUEL",
            "FULL","FUND","FUSE","FURY","FUSS","FUZZ","GAGS","GAIN","GAIT","GALE",
            "GAME","GANG","GAPE","GARB","GAVE","GAZE","GEAR","GENE","GIFT","GILD",
            "GILL","GIRL","GIST","GIVE","GLAD","GLEE","GLUE","GLUM","GNAW","GOAT",
            "GOES","GOLD","GOLF","GONE","GOOD","GORE","GRAB","GRAM","GRAY","GREW",
            "GRID","GRIM","GRIN","GRIP","GRIT","GROW","GRUB","GULF","GUST","GUTS",
            "HACK","HAIL","HAIR","HALE","HALF","HALL","HALT","HAND","HANG","HARD",
            "HARE","HARM","HARP","HATE","HAUL","HAVE","HAZE","HAZY","HEAD","HEAL",
            "HEAP","HEAR","HEAT","HEED","HEEL","HELD","HELM","HELP","HERD","HERE",
            "HERO","HERS","HIDE","HIGH","HIKE","HILL","HILT","HIND","HINT","HIRE",
            "HOLD","HOLE","HOME","HONE","HOOD","HOOK","HOPE","HORN","HOST","HOUR",
            "HOWL","HUGE","HULL","HUMP","HUNG","HUNT","HURL","HURT","HUSH","HYMN",
            "ICON","IDEA","IDLE","INCH","INTO","IRON","ISLE","ITEM","JACK","JADE",
            "JAIL","JARS","JAZZ","JEAN","JERK","JEST","JOBS","JOIN","JOKE","JOLT",
            "JUMP","JUNE","JURY","JUST","KEEN","KEEP","KELP","KEPT","KICK","KILL",
            "KIND","KING","KISS","KITE","KNACK","KNOB","KNOT","KNOW","LACE","LACK",
            "LAID","LAIN","LAKE","LAME","LAMP","LAND","LANE","LARD","LARK","LASH",
            "LASS","LAST","LATE","LAWN","LAZY","LEAD","LEAF","LEAK","LEAN","LEAP",
            "LEFT","LEND","LENS","LESS","LIED","LIEU","LIFE","LIFT","LIKE","LIMB",
            "LIME","LIMP","LINE","LINK","LION","LIST","LIVE","LOAD","LOAF","LOAN",
            "LOCK","LOFT","LONE","LONG","LOOK","LOOP","LORD","LORE","LOSE","LOSS",
            "LOST","LOTS","LOUD","LOVE","LUCK","LULL","LUMP","LURE","LURK","LUSH",
            "LUST","MACE","MADE","MAID","MAIL","MAIN","MAKE","MALE","MALT","MANE",
            "MANY","MARE","MARK","MARS","MASH","MASK","MASS","MAST","MATE","MAZE",
            "MEAD","MEAL","MEAN","MEET","MELD","MELT","MEMO","MEND","MERE","MESH",
            "MESS","MILD","MILE","MILK","MILL","MIME","MIND","MINE","MINT","MISS",
            "MIST","MOAN","MOAT","MOCK","MODE","MOLD","MOLT","MONK","MOOD","MOON",
            "MOOR","MORE","MOSS","MOST","MOTH","MOVE","MUCH","MUCK","MUFF","MULE",
            "MULL","MURK","MUSE","MUSH","MUSK","MUST","MUTE","MYTH","NAIL","NAME",
            "NAPE","NEAR","NEAT","NECK","NEED","NEST","NEXT","NICE","NINE","NODE",
            "NONE","NOON","NORM","NOSE","NOTE","NOUN","NUDE","NULL","NUMB","OBEY",
            "ODDS","OINK","OILY","OKAY","OMEN","OMIT","ONCE","ONLY","ONTO","OOZE",
            "OPEN","ORAL","OURS","OUST","OVEN","OVER","OWED","PACE","PACK","PACT",
            "PAGE","PAID","PAIL","PAIN","PAIR","PALE","PALM","PANE","PANG","PARK",
            "PART","PASS","PAST","PATH","PAVE","PEAK","PEAL","PEAR","PEAT","PECK",
            "PEEL","PEER","PELT","PEND","PERK","PEST","PICK","PIER","PILE","PILL",
            "PINE","PINK","PINT","PIPE","PITY","PLAN","PLAY","PLEA","PLOD","PLOT",
            "PLOW","PLOY","PLUG","PLUS","POKE","POLE","POLL","POLO","POMP","POND",
            "PONY","POOL","POOR","POPE","PORE","PORK","PORT","POSE","POST","POUR",
            "PRAY","PREP","PREY","PROD","PROP","PROW","PULL","PULP","PUMP","PUNK",
            "PURE","PUSH","QUAY","QUIT","QUIZ","RACE","RACK","RAFT","RAGE","RAID",
            "RAIL","RAIN","RAKE","RAMP","RANG","RANK","RANT","RARE","RASH","RATE",
            "RAVE","READ","REAL","REAM","REAP","REAR","REEF","REEL","REIN","RELY",
            "RENT","REST","RICH","RIDE","RIFT","RILE","RILL","RIND","RING","RIOT",
            "RISE","RISK","ROAD","ROAM","ROAR","ROBE","ROCK","RODE","ROLE","ROLL",
            "ROOF","ROOM","ROOT","ROPE","ROSE","ROSY","ROTE","ROUT","RUDE","RUIN",
            "RULE","RUMP","RUNG","RUSH","RUST","SAFE","SAGE","SAID","SAKE","SALE",
            "SALT","SAME","SAND","SANE","SANG","SANK","SASH","SAVE","SCAB","SCAM",
            "SCAN","SCAR","SEAL","SEAM","SEAR","SEAT","SECT","SEED","SEEK","SEEM",
            "SEEN","SELF","SELL","SEND","SENT","SHED","SHIN","SHIP","SHOD","SHOE",
            "SHOP","SHOT","SHOW","SHUT","SICK","SIDE","SIFT","SIGH","SIGN","SILK",
            "SILL","SILO","SILT","SING","SINK","SITE","SIZE","SKID","SKIM","SKIN",
            "SKIP","SLAB","SLAG","SLAM","SLAP","SLAT","SLAY","SLED","SLEW","SLID",
            "SLIM","SLIP","SLIT","SLOB","SLOP","SLOT","SLOW","SLUG","SLUM","SLUR",
            "SMOG","SNAP","SNAG","SNIP","SNOB","SNUB","SNUG","SOAK","SOAP","SOAR",
            "SOCK","SODA","SOFA","SOFT","SOIL","SOLD","SOLE","SOME","SONG","SOON",
            "SOOT","SORE","SORT","SOUL","SOUR","SPAN","SPAR","SPEC","SPED","SPIN",
            "SPIT","SPOT","SPUR","STAB","STAG","STAR","STAY","STEM","STEP","STEW",
            "STIR","STOP","STUB","STUD","STUN","SUCH","SUIT","SULK","SUMP","SUNG",
            "SUNK","SURE","SURF","SWAP","SWIM","SWUM","TACK","TACT","TAIL","TAKE",
            "TALE","TALK","TALL","TAME","TANG","TANK","TAPE","TART","TASK","TAXI",
            "TEAL","TEAM","TEAR","TELL","TEMP","TEND","TENT","TERM","TEST","TEXT",
            "THAN","THAT","THEM","THEN","THEY","THIN","THIS","THOU","THUD","THUS",
            "TICK","TIDE","TIDY","TIED","TIER","TILE","TILL","TILT","TIME","TINT",
            "TINY","TIRE","TOAD","TOES","TOIL","TOLD","TOLL","TOMB","TONE","TOOK",
            "TOOL","TOPS","TORE","TORN","TOSS","TOUR","TOWN","TRAP","TRAY","TREE",
            "TREK","TRIM","TRIO","TRIP","TROD","TROT","TRUE","TUCK","TUFT","TUNA",
            "TUNE","TURF","TURN","TWIG","TWIN","TWIT","TYPE","UGLY","UNDO","UNIT",
            "UPON","URGE","USED","USER","VAIN","VALE","VANE","VARY","VASE","VAST",
            "VEAL","VEER","VENT","VERB","VERY","VEST","VETO","VIEW","VILE","VINE",
            "VOID","VOLT","VOTE","WADE","WAGE","WAIL","WAIT","WAKE","WALK","WALL",
            "WAND","WANT","WARD","WARM","WARN","WARP","WART","WARY","WASH","WASP",
            "WAVE","WAVY","WAXY","WEAK","WEAN","WEAR","WEED","WEEK","WEEP","WELD",
            "WELL","WENT","WEPT","WERE","WEST","WHAT","WHEN","WHIM","WHIP","WHOM",
            "WICK","WIDE","WIFE","WILD","WILL","WILT","WILY","WIMP","WIND","WINE",
            "WING","WINK","WIPE","WIRE","WISE","WISH","WISP","WITH","WOKE","WOMB",
            "WOOD","WOOL","WORD","WORE","WORK","WORM","WORN","WOVE","WRAP","WREN",
            "YARD","YARN","YEAR","YELL","YOUR","YULE","ZEAL","ZERO","ZEST","ZINC","ZONE",

            // 5-letter words (≈700)
            "ABOUT","ABOVE","ABUSE","ACTOR","ACUTE","ADMIT","ADOPT","ADULT","AFTER","AGAIN",
            "AGENT","AGILE","AGING","AGREE","AHEAD","AISLE","ALARM","ALBUM","ALERT","ALIEN",
            "ALIGN","ALIKE","ALIVE","ALLEY","ALLOT","ALLOW","ALONE","ALONG","ALPHA","ALTER",
            "AMAZE","AMPLE","ANGEL","ANGER","ANGLE","ANGRY","ANKLE","ANNEX","ANTIC","APART",
            "ARENA","ARGUE","ARISE","ARMOR","AROMA","ARRAY","ASIDE","ASSET","ATTIC","AUDIO",
            "AVOID","AWAKE","AWARD","AWARE","BADGE","BADLY","BASIN","BASIS","BATCH","BEACH",
            "BEARD","BEAST","BEGIN","BEING","BELOW","BENCH","BERRY","BIRTH","BLACK","BLADE",
            "BLAME","BLAND","BLANK","BLAST","BLAZE","BLEAK","BLEED","BLEND","BLESS","BLIND",
            "BLINK","BLISS","BLOCK","BLOND","BLOOD","BLOOM","BLOWN","BOARD","BOAST","BONUS",
            "BOOTH","BOUND","BRACE","BRAIN","BRAND","BRAVE","BREAD","BREAK","BREED","BRICK",
            "BRIDE","BRIEF","BRING","BRINK","BRISK","BROAD","BROKE","BROOK","BROWN","BRUSH",
            "BUILD","BUILT","BUNCH","BURST","BUYER","CABIN","CABLE","CANDY","CARRY","CATCH",
            "CAUSE","CHAIN","CHAIR","CHAOS","CHARM","CHART","CHASE","CHEAP","CHEAT","CHECK",
            "CHEEK","CHEER","CHESS","CHEST","CHIEF","CHILD","CHILL","CHINA","CHUNK","CIVIL",
            "CLAIM","CLASH","CLASS","CLEAN","CLEAR","CLERK","CLICK","CLIFF","CLIMB","CLING",
            "CLOCK","CLONE","CLOSE","CLOTH","CLOUD","CLOWN","COACH","COAST","COLOR","COMMA",
            "CORAL","COUNT","COURT","COVER","CRACK","CRAFT","CRANE","CRASH","CRAWL","CRAZY",
            "CREAM","CREEK","CREEP","CREST","CRIME","CRISP","CROSS","CROWD","CROWN","CRUEL",
            "CRUSH","CURVE","CYCLE","DAILY","DANCE","DATUM","DEATH","DEBUT","DECAY","DELAY",
            "DENSE","DEPTH","DEVIL","DIARY","DIRTY","DISCO","DODGE","DOUBT","DOUGH","DRAFT",
            "DRAIN","DRAMA","DRANK","DRAPE","DRAWN","DREAM","DRESS","DRIED","DRIFT","DRILL",
            "DRINK","DRIVE","DROWN","DRUM","DRUNK","DRYER","DUMBO","DUSTY","DWARF","DWELL",
            "EAGER","EAGLE","EARTH","EASED","EIGHT","ELDER","ELECT","ELITE","EMPTY","ENEMY",
            "ENJOY","ENTER","ENTRY","EQUAL","ERROR","EVENT","EVERY","EXACT","EXERT","EXILE",
            "EXIST","EXTRA","FABLE","FAITH","FALSE","FANCY","FATAL","FAULT","FEAST","FENCE",
            "FETCH","FEVER","FEWER","FIBER","FIELD","FIGHT","FINAL","FLAIR","FLAME","FLASH",
            "FLASK","FLESH","FLIES","FLING","FLOAT","FLOCK","FLOOD","FLOOR","FLOUR","FLUID",
            "FLUSH","FLUTE","FOCUS","FORCE","FORGE","FOUND","FRAME","FRANK","FRAUD","FRESH",
            "FRONT","FROST","FROZE","FRUIT","FULLY","FUNNY","GIANT","GIVEN","GLARE","GLASS",
            "GLEAM","GLOBE","GLOOM","GLORY","GLOSS","GLOVE","GOING","GRACE","GRADE","GRAIN",
            "GRAND","GRANT","GRAPE","GRASP","GRASS","GRAVE","GREAT","GREEN","GREET","GRIEF",
            "GRIND","GROAN","GROOM","GROSS","GROUP","GROVE","GROWN","GUARD","GUESS","GUEST",
            "GUIDE","GUILT","HABIT","HANDY","HAPPY","HARSH","HAUNT","HAVEN","HEART","HEAVY",
            "HENCE","HOBBY","HONOR","HORSE","HOTEL","HOUSE","HUMAN","HUMOR","HURRY","IDEAL",
            "IMAGE","IMPLY","INDEX","INNER","INPUT","ISSUE","IVORY","JEWEL","JOINT","JOKER",
            "JUDGE","JUICE","JUICY","KNOCK","KNOWN","LABEL","LABOR","LARGE","LASER","LATER",
            "LAUGH","LAYER","LEARN","LEASE","LEGAL","LEVEL","LIGHT","LIMIT","LINEN","LIVER",
            "LOCAL","LODGE","LOGIC","LONELY","LOOSE","LOVER","LOWER","LOYAL","LUCKY","LUNAR",
            "LUNCH","LUNGE","MAGIC","MAJOR","MAKER","MANOR","MARCH","MATCH","MAYOR","MEDIA",
            "MERGE","MERIT","METAL","MIDST","MIGHT","MINOR","MINUS","MIXED","MODEL","MONEY",
            "MONTH","MORAL","MOTOR","MOUNT","MOUSE","MOUTH","MOVED","MOVIE","MUDDY","MUSIC",
            "NAVAL","NERVE","NEVER","NIGHT","NOBLE","NOISE","NORTH","NOTED","NOVEL","NURSE",
            "OCEAN","OFFER","OFTEN","OLIVE","ONSET","OPERA","ORBIT","ORDER","OTHER","OUTER",
            "OWNER","OXIDE","PAINT","PANEL","PANIC","PAPER","PARTY","PASTE","PATCH","PAUSE",
            "PEACE","PEARL","PENNY","PHASE","PHONE","PHOTO","PIANO","PIECE","PILOT","PITCH",
            "PIXEL","PIZZA","PLACE","PLAIN","PLANE","PLANT","PLATE","PLAZA","PLEAD","PLUMB",
            "PLUME","POINT","POLAR","PORCH","POUCH","POUND","POWER","PRESS","PRICE","PRIDE",
            "PRIME","PRINT","PRIOR","PRIZE","PROBE","PROOF","PROSE","PROUD","PROVE","PROXY",
            "PULSE","PUNCH","PUPIL","QUEEN","QUERY","QUEST","QUEUE","QUICK","QUIET","QUOTA",
            "QUOTE","RADAR","RADIO","RAISE","RALLY","RANCH","RANGE","RAPID","RATIO","REACH",
            "REALM","REBEL","REIGN","RELAX","RENEW","REPLY","RIDER","RIDGE","RIGHT","RIGID",
            "RISKY","RIVAL","RIVER","ROBIN","ROBOT","ROCKY","ROMAN","ROUGH","ROUND","ROUTE",
            "ROYAL","RUGBY","RULER","RURAL","SAINT","SALAD","SAUCE","SCALE","SCARE","SCENE",
            "SCENT","SCOPE","SCORE","SCOUT","SCRAP","SERVE","SEVEN","SHADE","SHAKE","SHALL",
            "SHAME","SHAPE","SHARE","SHARK","SHARP","SHEAR","SHEET","SHELF","SHELL","SHIFT",
            "SHINE","SHIRT","SHOCK","SHOOT","SHORE","SHORT","SHOUT","SHOWN","SIGHT","SINCE",
            "SIXTH","SIXTY","SIZED","SKILL","SKULL","SLATE","SLEEP","SLICE","SLIDE","SLOPE",
            "SMALL","SMART","SMELL","SMILE","SMOKE","SNACK","SNAKE","SOLAR","SOLID","SOLVE",
            "SORRY","SOUND","SOUTH","SPACE","SPARE","SPARK","SPEAK","SPEED","SPELL","SPEND",
            "SPENT","SPICE","SPIKE","SPINE","SPLIT","SPOKE","SPOON","SPORT","SPRAY","SQUAD",
            "STACK","STAFF","STAGE","STAIN","STAKE","STALE","STALL","STAMP","STAND","STARE",
            "START","STATE","STAY","STEAK","STEAL","STEAM","STEEL","STEEP","STEER","STERN",
            "STICK","STIFF","STILL","STOCK","STOLE","STONE","STOOD","STORE","STORM","STORY",
            "STOVE","STUFF","STYLE","SUGAR","SUITE","SUNNY","SUPER","SURGE","SWAMP","SWEAR",
            "SWEET","SWEPT","SWIFT","SWING","SWORD","TASTE","TEACH","TEETH","THANK","THEME",
            "THERE","THICK","THIEF","THING","THINK","THIRD","THOSE","THREE","THREW","THROW",
            "THUMB","TIGER","TIGHT","TIMER","TIRED","TITLE","TODAY","TOKEN","TOTAL","TOUCH",
            "TOUGH","TOWER","TOXIC","TRACE","TRACK","TRADE","TRAIL","TRAIN","TRAIT","TRASH",
            "TREAT","TREND","TRIAL","TRIBE","TRICK","TRIED","TROOP","TRUCK","TRULY","TRUNK",
            "TRUST","TRUTH","TUMOR","TUNER","TWICE","TWIST","ULTRA","UNCLE","UNDER","UNIFY",
            "UNION","UNITE","UNITY","UNTIL","UPPER","UPSET","URBAN","USAGE","USUAL","UTTER",
            "VALID","VALUE","VAULT","VERSE","VIDEO","VIGOR","VIRAL","VIRUS","VISIT","VITAL",
            "VIVID","VOCAL","VOICE","VOTER","WAGE","WAGON","WASTE","WATCH","WATER","WEAVE",
            "WEIGH","WEIRD","WHALE","WHEAT","WHEEL","WHERE","WHICH","WHILE","WHITE","WHOLE",
            "WHOSE","WIDER","WITCH","WOMAN","WOMEN","WORLD","WORRY","WORSE","WORST","WORTH",
            "WOULD","WOUND","WRIST","WRITE","WRONG","WROTE","YIELD","YOUNG","YOUTH","ZEBRA"
        };

        return new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);
    }
}
