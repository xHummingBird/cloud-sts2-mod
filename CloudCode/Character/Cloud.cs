using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Cloud.CloudCode.Cards.Ancient;
using Cloud.CloudCode.Cards.Basic;
using Cloud.CloudCode.Cards.Common;
using Cloud.CloudCode.Cards.Rare;
using Cloud.CloudCode.Cards.Uncommon;
using Cloud.CloudCode.Extensions;
using Cloud.CloudCode.Mechanics.ATB;
using Cloud.CloudCode.Mechanics.Limit;
using Cloud.CloudCode.Mechanics.Summon;
using Cloud.CloudCode.Powers;
using Cloud.CloudCode.Relics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Cloud.CloudCode.Character;

public class Cloud : PlaceholderCharacterModel
{
	public const string CharacterId = "Cloud";

	public static readonly Color Color = new("ffffff");
	private Vector2? _originalPosition;
	public override Color NameColor => Color;
	public override CharacterGender Gender => CharacterGender.Masculine;
	public override int StartingHp => 80;

	public override IEnumerable<CardModel> StartingDeck =>
	[
		ModelDb.Card<StrikeCloud>(),
		ModelDb.Card<Braver>(),
		ModelDb.Card<StrikeCloud>(),
		ModelDb.Card<StrikeCloud>(),
		ModelDb.Card<StrikeCloud>(),
		ModelDb.Card<GuardBreak>(),
		ModelDb.Card<GuardStance>(),
		ModelDb.Card<DefendCloud>(),
		ModelDb.Card<DefendCloud>(),
		ModelDb.Card<DefendCloud>(),
		ModelDb.Card<DefendCloud>(),
		ModelDb.Card<ModeShift>(),
	];

	public override IReadOnlyList<RelicModel> StartingRelics =>
	[
		ModelDb.Relic<BusterSword>()
	];

	public override CardPoolModel CardPool => ModelDb.CardPool<CloudCardPool>();
	public override RelicPoolModel RelicPool => ModelDb.RelicPool<CloudRelicPool>();
	public override PotionPoolModel PotionPool => ModelDb.PotionPool<CloudPotionPool>();

	public override Control CustomIcon
	{
		get
		{
			var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
			icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
			return icon;
		}
	}

	public override CustomEnergyCounter? CustomEnergyCounter =>
		new CustomEnergyCounter(EnergyCounterPaths, new Color(0.2f, 0.2f, 0.2f), new Color(1f, 1f, 1f));
	
	private string EnergyCounterPaths(int i)
	{
		return i switch
		{
			1 => "charui/big_energy.png".ImagePath(),
			_ => "charui/blank.png".ImagePath()
		};
	}

	private const string CustomVisualScenePath = "res://Cloud/scenes/cloud.tscn";
	public override string CustomRestSiteAnimPath => "res://Cloud/scenes/Cloud_rest_site.tscn";
	public override string CustomCharacterSelectBg => "char_selection_bg_cloud.tscn".CharacterUiPath();
	public override string CustomMerchantAnimPath => "res://Cloud/scenes/Cloud_merchant.tscn";
	public override string CustomIconTexturePath => "character_icon_cloud.png".CharacterUiPath();
	public override string CustomCharacterSelectIconPath => "char_select_cloud.png".CharacterUiPath();
	public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
	public override string CustomMapMarkerPath => "map_marker_cloud_2.png".CharacterUiPath();
	public override string CharacterSelectSfx => "res://Cloud/sounds/not_interested.wav";
	public override string CharacterTransitionSfx => "res://Cloud/sfx/sword_swing_heavy.wav";

	public override string CustomCharacterSelectTransitionPath =>
		"res://Cloud/images/transition/cloud_transition_mat.tres";

	public override NCreatureVisuals? CreateCustomVisuals()
	{
		CloudAssets.EnsurePreloaded();
		return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualScenePath);
	}

	public override CreatureAnimator? GenerateAnimator(MegaSprite controller) => null;

	private string GetIdleAnimation(Creature creature)
	{
		var mode = creature.IsPunisher() ? "punisher" : "operator";
		var prime = creature.IsPrime() ? "prime_" : "";

		return $"idle_{prime}{mode}";
	}

	public (float total, float[] impacts) PlayAnimation(Creature creature, string trigger)
	{

		if (creature == null || string.IsNullOrEmpty(trigger))
			return (0f, Array.Empty<float>());

		var node = NCombatRoom.Instance?.GetCreatureNode(creature);
		if (node?.Visuals == null)
			return (0f, Array.Empty<float>());

		var animPlayer = node.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (animPlayer == null)
			return (0f, Array.Empty<float>());

		var t = trigger.ToLowerInvariant();
		var mode = creature.IsPunisher() ? "punisher" : "operator";
		var prime = creature.IsPrime() ? "_prime" : "";

		string godotTrigger = t switch
		{

			"attack" => $"attack_{mode}",

			"block" => $"block{prime}",

			"dash" => $"dash_{mode}",

			("idle") or ("idle_loop") => GetIdleAnimation(creature),

			"dead" or "die" => "dead",

			"cast" => $"cast_{mode}",

			_ => trigger
		};

		if (!animPlayer.HasAnimation(godotTrigger))
			return (0f, Array.Empty<float>());

		bool shouldRestartIfAlreadyPlaying =
			t == "attack" || t == "cast";


		if (shouldRestartIfAlreadyPlaying && animPlayer.CurrentAnimation == godotTrigger)
		{
			animPlayer.Seek(0, true);
		}

		else
		{
			animPlayer.Play(godotTrigger);
		}

		var anim = animPlayer.GetAnimation(godotTrigger);
		float totalLength = (float)anim.Length;

		if (godotTrigger is not ("die" or "dead"))
		{
			string nextIdle = GetIdleAnimation(creature);
			animPlayer.Queue(nextIdle);
		}

		return (totalLength, Array.Empty<float>());
	}

	public void RefreshIdle(Creature creature)
	{
		var node = NCombatRoom.Instance?.GetCreatureNode(creature);
		var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

		if (animPlayer == null)
			return;

		string idle = GetIdleAnimation(creature);

		if (animPlayer.CurrentAnimation != idle)
			animPlayer.Play(idle);
	}

	public void DoScreenShake(ShakeStrength strength = ShakeStrength.Medium,
		ShakeDuration duration = ShakeDuration.Short)
	{
		NGame.Instance?.ScreenShake(strength, duration);
	}

	public Node2D PlayVfxOnTarget(Creature target, string path, string animName)
	{
		var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
		if (targetNode?.Visuals == null)
			return null;

		var scene = GD.Load<PackedScene>(path);
		var vfx = scene.Instantiate<Node2D>();

		targetNode.Visuals.AddChild(vfx);
		vfx.Position = Vector2.Zero;

		var animPlayer = vfx.GetNode<AnimationPlayer>("AnimationPlayer");

		if (animPlayer.HasAnimation(animName))
			animPlayer.Play(animName);

		return vfx;
	}



	private string ResolveDashAnimation(Creature creature)
	{
		return creature.IsPunisher() ? "dash_punisher" : "dash_operator";
	}

	
	
	public async Task DashTo(
		Creature player,
		Creature target,
		float durationSeconds = 0.3f,
		float distance = 200f,
		bool dashBehind = false,
		string? overrideAnim = null)
	{
		var node = NCombatRoom.Instance?.GetCreatureNode(player);
		var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
		if (node == null || targetNode == null) return;

		if (!_originalPosition.HasValue)
			_originalPosition = node.GlobalPosition;

		string anim = overrideAnim ?? ResolveDashAnimation(player);
		PlayAnimation(player, anim);
		
		bool playerIsLeftOfTarget = node.GlobalPosition.X < targetNode.GlobalPosition.X;
		
		Vector2 offsetDir = playerIsLeftOfTarget ? Vector2.Left : Vector2.Right;
		
		if (dashBehind)
			offsetDir = -offsetDir;

		Vector2 targetPos = targetNode.GlobalPosition + offsetDir * distance;

		var tween = node.CreateTween();
		tween.TweenProperty(node, "global_position", targetPos, durationSeconds)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);

		await node.ToSignal(tween, Tween.SignalName.Finished);
	}




	public async Task DashPast(
		Creature player,
		Creature target,
		string? attackAnim = null,
		float durationSeconds = 0.3f,
		float behindDistance = 200f,
		float overshoot = 0f)
	{
		var node = NCombatRoom.Instance?.GetCreatureNode(player);
		var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
		if (node == null || targetNode == null) return;

		if (!_originalPosition.HasValue)
			_originalPosition = node.GlobalPosition;

		Vector2 frontDir = (player.Side == CombatSide.Player) ? Vector2.Left : Vector2.Right;
		Vector2 behindDir = -frontDir;

		Vector2 endPos = targetNode.GlobalPosition + behindDir * (behindDistance + overshoot);

		PlayAnimation(player, attackAnim);

		var tween = node.CreateTween();
		tween.TweenProperty(node, "global_position", endPos, durationSeconds)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.Out);

		await node.ToSignal(tween, Tween.SignalName.Finished);
	}


	public async Task Retreat(Creature player)
	{
		var node = NCombatRoom.Instance?.GetCreatureNode(player);
		if (node == null || !_originalPosition.HasValue) return;

		PlayAnimation(player, "retreat");

		var tween = node.CreateTween();
		tween.TweenProperty(node, "global_position", _originalPosition.Value, 0.3f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.InOut);

		await node.ToSignal(tween, Tween.SignalName.Finished);

		_originalPosition = null;

		var visuals = node.Visuals.GetNodeOrNull<Node2D>("Visuals");
		if (visuals != null)
			visuals.Position = Vector2.Zero;

		PlayAnimation(player, "idle");
	}

	[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
	public static class NCreatureSetTriggerPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(NCreature __instance, string trigger)
		{
			if (__instance.Entity?.Player?.Character is Cloud character)
			{
				character.PlayAnimation(__instance.Entity, trigger);
				return false;
			}

			return true;
		}
	}

	[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
	public static class StartDeathAnimPatch
	{
		[HarmonyPostfix]
		public static void Postfix(NCreature __instance, ref float __result)
		{
			if (__instance.Entity?.Player?.Character is Cloud character)
			{
				AudioHelper.PlayRandomGameover();
				character.PlayAnimation(__instance.Entity, "die");

				var animPlayer = __instance.Visuals.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
				__result = animPlayer?.GetAnimation("die")?.Length ?? 1.5f;
			}
		}
	}
	
	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatVictory))]
	public static class CloudVictoryAnimationPatch
	{
		[HarmonyPostfix]
		public static void Postfix(IRunState runState, CombatState? combatState)
		{

			var creatures = combatState?.Creatures?.Where(c => c.IsPlayer);

			if (creatures == null)
				return;

			foreach (var creature in creatures)
			{
				if (creature.Player?.Character is not Cloud)
					continue;

				var node = NCombatRoom.Instance?.GetCreatureNode(creature);
				var animPlayer = node?.Visuals?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

				ATBManager.SetATB(creature.Player, 0);
				ATBManager.Reset(creature.Player);
				LimitManager.HalfLimit(creature.Player);
				SummonManager.HalfSummon(creature.Player);

				if (animPlayer == null)
					continue;

				if (animPlayer.HasAnimation("victory_before"))
				{
					if (animPlayer.CurrentAnimation == "omnislash_ver_5")
					{
						SfxCmd.Play("res://Cloud/sounds/omnislashver5_end.wav");
					}
					else
					{
						AudioHelper.PlayRandomVictory();
					}

					animPlayer.Play("victory_before");

					if (animPlayer.HasAnimation("victory"))
						animPlayer.Queue("victory");
				}
				else
				{
					animPlayer.Play("victory");
				}
			}
		}
	}

	[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageReceived))]
	public static class CloudDamageAnimationPatch
	{
		[HarmonyPostfix]
		public static void Postfix(Creature target, DamageResult result, ValueProp props, Creature? dealer)
		{
			if (target.Player?.Character is not Cloud character)
				return;


			if (dealer == null || dealer.Side != CombatSide.Enemy)
				return;


			if (props.HasFlag(ValueProp.SkipHurtAnim) || props.HasFlag(ValueProp.Unpowered))
				return;

			if (result.WasFullyBlocked && result.BlockedDamage > 0)
			{
				character.PlayAnimation(target, "block");
			}


			else if (result.UnblockedDamage > 0 && !target.IsDead)
			{
				character.PlayAnimation(target, "block");
				if (target.CurrentHp < 20)
				{
					AudioHelper.PlayRandomDamagedCritical();
				}
				else if (result.UnblockedDamage < 10)
				{
					AudioHelper.PlayRandomDamaged();
				}
				else
				{
					AudioHelper.PlayRandomDamagedHigh();
				}
			}
		}
	}



	[HarmonyPatch(typeof(CardSelectCmd), nameof(CardSelectCmd.FromChooseACardScreen))]
	public static class CardSelectCmd_FromChooseACardScreen_Patch
	{
		[HarmonyPrefix]
		public static bool Prefix(
			PlayerChoiceContext context,
			IReadOnlyList<CardModel> cards,
			Player player,
			bool canSkip,
			ref Task<CardModel?> __result)
		{
			__result = PatchedChoose(context, cards, player, canSkip);
			return false; 
		}

		private static async Task<CardModel?> PatchedChoose(
			PlayerChoiceContext context,
			IReadOnlyList<CardModel> cards,
			Player player,
			bool canSkip)
		{
			if (cards.Count > 5)
			{
				throw new ArgumentException("Only works with 5 or fewer cards", nameof(cards));
			}

			if (cards.Count == 0)
			{
				return null;
			}

			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);

			await context.SignalPlayerChoiceBegun(player, PlayerChoiceOptions.None);

			CardModel? result;
			
			if (LocalContext.IsMe(player))
			{
				NPlayerHand.Instance?.CancelAllCardPlay();

				var screen = NChooseACardSelectionScreen.ShowScreen(cards, canSkip);

				if (screen == null)
				{
					await context.SignalPlayerChoiceEnded();
					return null;
				}
				
				foreach (var card in cards)
				{
					SaveManager.Instance.MarkCardAsSeen(card);
				}

				result = (await screen.CardsSelected()).FirstOrDefault();

				int index = cards.IndexOf(result);
				var choiceResult = PlayerChoiceResult.FromIndex(index);

				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, choiceResult);
			}
			else
			{
				int index = (await RunManager.Instance.PlayerChoiceSynchronizer
						.WaitForRemoteChoice(player, choiceId))
					.AsIndex();

				result = index < 0 ? null : cards[index];
			}

			await context.SignalPlayerChoiceEnded();

			return result;
		}
	}
}
