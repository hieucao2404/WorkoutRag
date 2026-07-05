import matplotlib.pyplot as plt
import matplotlib.patches as mpatches
from matplotlib.patches import FancyBboxPatch, FancyArrowPatch

fig, ax = plt.subplots(figsize=(14, 12))
ax.set_xlim(0, 14)
ax.set_ylim(0, 12)
ax.axis('off')
fig.patch.set_facecolor('white')

# ── Colors ──────────────────────────────────────────────────────────────────
ORANGE_FILL  = '#FDE8C8'
ORANGE_EDGE  = '#E07B00'
BLUE_FILL    = '#C8DFFE'
BLUE_EDGE    = '#1A6FD4'
GRAY_FILL    = '#EFEFEF'
GRAY_EDGE    = '#888888'
TEXT_COLOR   = '#1A1A1A'

# ── Helper: rounded box ──────────────────────────────────────────────────────
def draw_box(ax, x, y, w, h, label, sublabel='',
             fc=ORANGE_FILL, ec=ORANGE_EDGE):
    box = FancyBboxPatch((x, y), w, h,
                         boxstyle="round,pad=0.08",
                         facecolor=fc, edgecolor=ec,
                         linewidth=1.6, zorder=3)
    ax.add_patch(box)
    cx, cy = x + w / 2, y + h / 2
    if sublabel:
        ax.text(cx, cy + 0.13, label,
                ha='center', va='center',
                fontsize=10, fontweight='bold', color=TEXT_COLOR, zorder=4)
        ax.text(cx, cy - 0.18, sublabel,
                ha='center', va='center',
                fontsize=7.5, color='#444444', zorder=4)
    else:
        ax.text(cx, cy, label,
                ha='center', va='center',
                fontsize=10, fontweight='bold', color=TEXT_COLOR, zorder=4)

# ── Helper: arrow ────────────────────────────────────────────────────────────
def draw_arrow(ax, x, y_start, y_end, color='#555555'):
    ax.annotate('', xy=(x, y_end + 0.02),
                xytext=(x, y_start - 0.02),
                arrowprops=dict(arrowstyle='->', color=color,
                                lw=1.8), zorder=2)

# ════════════════════════════════════════════════════════════════════════════
# COLUMN POSITIONS
# ════════════════════════════════════════════════════════════════════════════
BOX_W  = 4.0
BOX_H  = 0.80
GAP    = 0.45          # vertical gap between boxes
LX     = 0.8           # left  pipeline x-start
RX     = 8.2           # right pipeline x-start
LCX    = LX + BOX_W / 2   # left  centre x
RCX    = RX + BOX_W / 2   # right centre x

# ── Titles ───────────────────────────────────────────────────────────────────
ax.text(LCX, 11.55, 'LLM-Only Baseline',
        ha='center', va='center', fontsize=13,
        fontweight='bold', color=ORANGE_EDGE)
ax.text(RCX, 11.55, 'WorkoutRAG (Proposed)',
        ha='center', va='center', fontsize=13,
        fontweight='bold', color=BLUE_EDGE)

# ── Vertical divider ─────────────────────────────────────────────────────────
ax.plot([7, 7], [0.6, 11.2], color='#AAAAAA', linewidth=1.4,
        linestyle='--', zorder=1)
ax.text(7, 6.0, 'Only architectural\ndifference:\nRetrieval Stage',
        ha='center', va='center', fontsize=8,
        color='#666666', style='italic',
        rotation=90)

# ════════════════════════════════════════════════════════════════════════════
# LEFT PIPELINE  (LLM-Only) — 4 boxes
# ════════════════════════════════════════════════════════════════════════════
left_steps = [
    ('User Profile',
     'Demographics · Physiological\nBehavioral · Workout-specific'),
    ('Prompt Builder',
     'Profile → Structured Prompt'),
    ('Phi-3  (Ollama)',
     'LLM — Zero-shot generation'),
    ('Workout Plan JSON',
     'Structured Output'),
]

# Space 4 boxes evenly between y = 1.4 and y = 10.8
l_tops = [9.8, 7.6, 5.4, 3.2]

for i, (label, sub) in enumerate(left_steps):
    draw_box(ax, LX, l_tops[i], BOX_W, BOX_H,
             label, sub, fc=ORANGE_FILL, ec=ORANGE_EDGE)
    if i < len(left_steps) - 1:
        draw_arrow(ax, LCX,
                   l_tops[i],          # bottom of current box
                   l_tops[i+1] + BOX_H,  # top of next box
                   color=ORANGE_EDGE)

# ════════════════════════════════════════════════════════════════════════════
# RIGHT PIPELINE  (WorkoutRAG) — 7 boxes
# ════════════════════════════════════════════════════════════════════════════
right_steps = [
    ('User Profile',
     'Demographics · Physiological\nBehavioral · Workout-specific'),
    ('Query Embedding',
     'nomic-embed-text'),
    ('pgvector Similarity Search',
     'PostgreSQL + pgvector · Cosine Distance'),
    ('Top-K Exercise Retrieval',
     'Semantically relevant exercises from DB'),
    ('Prompt Builder',
     'Profile + Retrieved Exercises → Grounded Prompt'),
    ('Phi-3  (Ollama)',
     'LLM — Grounded generation'),
    ('Workout Plan JSON',
     'Structured Output'),
]

r_tops = [9.8, 8.55, 7.30, 6.05, 4.80, 3.55, 2.30]

for i, (label, sub) in enumerate(right_steps):
    draw_box(ax, RX, r_tops[i], BOX_W, BOX_H,
             label, sub, fc=BLUE_FILL, ec=BLUE_EDGE)
    if i < len(right_steps) - 1:
        draw_arrow(ax, RCX,
                   r_tops[i],
                   r_tops[i+1] + BOX_H,
                   color=BLUE_EDGE)

# ════════════════════════════════════════════════════════════════════════════
# SHARED OUTPUT BOX  (bottom, spans both columns)
# ════════════════════════════════════════════════════════════════════════════
shared_y = 0.65
draw_box(ax, 0.5, shared_y, 13.0, 0.72,
         'Shared Output Schema',
         'workoutName · duration · goal · exercises [ name, sets, reps, rest, notes ]',
         fc=GRAY_FILL, ec=GRAY_EDGE)

# connecting arrows from both bottom boxes to shared box
for cx, bot_y in [(LCX, l_tops[-1]), (RCX, r_tops[-1])]:
    draw_arrow(ax, cx, bot_y, shared_y + 0.72, color='#888888')

# ── Figure caption ────────────────────────────────────────────────────────────
ax.text(7, 0.22,
        'Figure 1: Overall architecture of the WorkoutRAG framework compared with the LLM-only baseline.',
        ha='center', va='center', fontsize=9, color='#333333',
        style='italic')

plt.tight_layout()
plt.savefig('workoutrag_figure1.png', dpi=300, bbox_inches='tight',
            facecolor='white')
print("Saved: workoutrag_figure1.png")
plt.show()

