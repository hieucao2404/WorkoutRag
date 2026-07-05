import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch

fig, ax = plt.subplots(figsize=(10, 9))
ax.set_xlim(0, 10)
ax.set_ylim(0, 9)
ax.axis('off')
fig.patch.set_facecolor('white')

# ── Colors ──────────────────────────────────────────────────────────────────
ORANGE_FILL = '#FDE8C8'; ORANGE_EDGE = '#E07B00'
BLUE_FILL   = '#C8DFFE'; BLUE_EDGE   = '#1A6FD4'
GREEN_FILL  = '#D4EDDA'; GREEN_EDGE  = '#2E7D4F'
GRAY_FILL   = '#EFEFEF'; GRAY_EDGE   = '#888888'
TEXT_COLOR  = '#1A1A1A'

def box(ax, x, y, w, h, label, sub='', fc=GRAY_FILL, ec=GRAY_EDGE, fs=10):
    ax.add_patch(FancyBboxPatch((x, y), w, h,
                 boxstyle="round,pad=0.1", facecolor=fc,
                 edgecolor=ec, linewidth=1.8, zorder=3))
    cx, cy = x + w/2, y + h/2
    if sub:
        ax.text(cx, cy+0.14, label, ha='center', va='center',
                fontsize=fs, fontweight='bold', color=TEXT_COLOR, zorder=4)
        ax.text(cx, cy-0.17, sub, ha='center', va='center',
                fontsize=7.5, color='#555', zorder=4)
    else:
        ax.text(cx, cy, label, ha='center', va='center',
                fontsize=fs, fontweight='bold', color=TEXT_COLOR, zorder=4)

def arrow(ax, x1, y1, x2, y2, color='#555'):
    ax.annotate('', xy=(x2, y2), xytext=(x1, y1),
                arrowprops=dict(arrowstyle='->', color=color,
                                lw=1.8), zorder=2)

def line(ax, x1, y1, x2, y2, color='#555', style='-'):
    ax.plot([x1, x2], [y1, y2], color=color,
            lw=1.8, linestyle=style, zorder=2)

# ── Shared top: WorkoutRequest ───────────────────────────────────────────────
box(ax, 3.25, 7.5, 3.5, 0.75,
    'WorkoutRequest', '(Shared Input — identical for both)',
    fc=GRAY_FILL, ec=GRAY_EDGE)

# Fork lines down to two branches
#  from centre-bottom of WorkoutRequest
line(ax, 5.0, 7.5, 5.0, 7.0, '#888')   # short stem
line(ax, 2.5, 7.0, 7.5, 7.0, '#888')   # horizontal bar
line(ax, 2.5, 7.0, 2.5, 6.5, ORANGE_EDGE)  # left drop
line(ax, 7.5, 7.0, 7.5, 6.5, BLUE_EDGE)   # right drop
arrow(ax, 2.5, 6.5, 2.5, 6.42, ORANGE_EDGE)
arrow(ax, 7.5, 6.5, 7.5, 6.42, BLUE_EDGE)

# ── LEFT BRANCH: LLM-Only ───────────────────────────────────────────────────
ax.text(2.5, 6.8, 'LLM-Only Baseline',
        ha='center', fontsize=9.5, fontweight='bold', color=ORANGE_EDGE)

box(ax, 0.5, 5.5, 4.0, 0.75,
    'Prompt Builder', 'User profile → structured prompt',
    fc=ORANGE_FILL, ec=ORANGE_EDGE)

arrow(ax, 2.5, 5.5, 2.5, 5.42, ORANGE_EDGE)

# ── RIGHT BRANCH: WorkoutRAG ─────────────────────────────────────────────────
ax.text(7.5, 6.8, 'WorkoutRAG (Proposed)',
        ha='center', fontsize=9.5, fontweight='bold', color=BLUE_EDGE)

box(ax, 5.5, 5.5, 4.0, 0.75,
    'Vector Retrieval',
    'nomic-embed-text → pgvector → Top-K exercises',
    fc=BLUE_FILL, ec=BLUE_EDGE)

arrow(ax, 7.5, 5.5, 7.5, 5.42, BLUE_EDGE)

box(ax, 5.5, 4.5, 4.0, 0.75,
    'Prompt Builder',
    'User profile + retrieved exercises → grounded prompt',
    fc=BLUE_FILL, ec=BLUE_EDGE)

arrow(ax, 7.5, 4.5, 7.5, 4.42, BLUE_EDGE)

# ── Merge lines into shared Phi-3 ────────────────────────────────────────────
# Left branch arrow ends at y=5.5 bottom → merge bar at y=4.0
arrow(ax, 2.5, 5.5, 2.5, 4.27, ORANGE_EDGE)   # left straight down to bar
line(ax, 2.5, 4.12, 7.5, 4.12, '#888')          # horizontal merge bar
line(ax, 7.5, 4.5,  7.5, 4.12, BLUE_EDGE)       # right branch into bar
line(ax, 5.0, 4.12, 5.0, 3.85, '#555')          # stem to Phi-3
arrow(ax, 5.0, 3.85, 5.0, 3.77, '#555')

# ── Shared: Phi-3 ────────────────────────────────────────────────────────────
box(ax, 3.0, 2.85, 4.0, 0.75,
    'Phi-3  (Ollama)',
    'Same model · same temperature · same parameters',
    fc=GREEN_FILL, ec=GREEN_EDGE)

arrow(ax, 5.0, 2.85, 5.0, 2.77, GREEN_EDGE)

# ── Shared: Output ───────────────────────────────────────────────────────────
box(ax, 3.0, 1.8, 4.0, 0.75,
    'Workout Plan JSON',
    'workoutName · duration · goal · exercises',
    fc=GREEN_FILL, ec=GREEN_EDGE)

# ── Legend ───────────────────────────────────────────────────────────────────
legend_items = [
    mpatches.Patch(facecolor=ORANGE_FILL, edgecolor=ORANGE_EDGE, label='LLM-Only Baseline'),
    mpatches.Patch(facecolor=BLUE_FILL,   edgecolor=BLUE_EDGE,   label='WorkoutRAG (RAG components)'),
    mpatches.Patch(facecolor=GREEN_FILL,  edgecolor=GREEN_EDGE,  label='Shared Components'),
]
ax.legend(handles=legend_items, loc='lower left',
          bbox_to_anchor=(0.01, 0.01), fontsize=8.5, framealpha=0.9)

# ── Caption ──────────────────────────────────────────────────────────────────
ax.text(5.0, 1.35,
        'Figure 2: Comparison of LLM-Only and WorkoutRAG pipelines sharing the same input and output.',
        ha='center', va='center', fontsize=8.5, color='#333', style='italic')

plt.tight_layout()
plt.savefig('workoutrag_figure2.png', dpi=300, bbox_inches='tight', facecolor='white')
print("Saved: workoutrag_figure2.png")

