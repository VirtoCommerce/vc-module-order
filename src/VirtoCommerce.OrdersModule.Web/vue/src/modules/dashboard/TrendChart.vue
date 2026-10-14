<!-- Area spline after Dashboard.jsx Sparkline: hover band, marker and tooltip, pill on the hovered label.
     Unlike the design it is zero-based (min-max scaling exaggerates small changes) and has light gridlines. -->
<template>
  <div
    :ref="observe"
    class="trend-chart"
    @mousemove="onMove"
    @mouseleave="hover = null"
  >
    <svg
      :width="width"
      :height="height"
      :viewBox="`0 0 ${width} ${height}`"
      role="img"
      :aria-label="ariaLabel"
    >
      <defs>
        <linearGradient
          :id="fillId"
          x1="0"
          y1="0"
          x2="0"
          y2="1"
        >
          <stop
            offset="0%"
            stop-color="#3BA3DF"
            stop-opacity="0.28"
          />
          <stop
            offset="100%"
            stop-color="#3BA3DF"
            stop-opacity="0.02"
          />
        </linearGradient>
      </defs>

      <g
        v-for="tick in ticks"
        :key="tick.y"
      >
        <line
          :x1="PAD_X"
          :x2="width - PAD_X"
          :y1="tick.y"
          :y2="tick.y"
          class="trend-chart__grid"
        />
        <text
          v-if="tick.label"
          :x="PAD_X"
          :y="tick.y - 6"
          class="trend-chart__tick"
        >
          {{ tick.label }}
        </text>
      </g>

      <rect
        v-if="selected"
        :x="selected.x - step / 2"
        y="0"
        :width="step"
        :height="bottom"
        fill="#EAF5FC"
      />
      <path
        :d="area"
        :fill="`url(#${fillId})`"
      />
      <path
        :d="line"
        fill="none"
        stroke="#3BA3DF"
        stroke-width="3"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
      <circle
        v-if="selected"
        :cx="selected.x"
        :cy="selected.y"
        r="6"
        fill="#fff"
        stroke="#E9A23B"
        stroke-width="3"
      />

      <g
        v-for="(label, index) in labels"
        :key="label + index"
      >
        <rect
          v-if="index === hover"
          :x="xOf(index) - 26"
          :y="bottom + 11"
          width="52"
          height="24"
          rx="12"
          fill="#86C7EA"
        />
        <text
          :x="xOf(index)"
          :y="bottom + 28"
          text-anchor="middle"
          class="trend-chart__label"
          :class="{ 'trend-chart__label--active': index === hover }"
        >
          {{ label }}
        </text>
      </g>
    </svg>

    <div
      v-if="selected && hover !== null"
      class="trend-chart__tip"
      :style="{ left: `${tipX}px`, top: `${selected.y - 16}px` }"
    >
      <strong>{{ format(values[hover]) }}</strong>
      <span
        v-if="hover > 0 && values[hover] !== values[hover - 1]"
        class="dash-delta"
        :class="{ 'dash-delta--down': values[hover] < values[hover - 1] }"
      >
        {{ format(values[hover] - values[hover - 1], true) }}
      </span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, onBeforeUnmount, ref, watch } from "vue";

const props = defineProps<{
  values: number[];
  labels: string[];
  format: (value: number, signed?: boolean) => string;
  ariaLabel?: string;
}>();

const PAD_TOP = 58;
const PAD_BOTTOM = 46;
const PAD_X = 22; // room for half a month label at either end

const width = ref(640);
const height = ref(280);
const hover = ref<number | null>(null);
const fillId = `trend-fill-${Math.random().toString(36).slice(2)}`;

// a function ref: object template refs don't bind in this Module Federation remote
// (the element comes back undefined in onMounted)
let observer: ResizeObserver | undefined;
function observe(element: unknown) {
  observer?.disconnect();
  observer = undefined;
  if (!(element instanceof HTMLElement)) return;
  observer = new ResizeObserver(([entry]) => {
    if (entry.contentRect.width && entry.contentRect.height) {
      width.value = entry.contentRect.width;
      height.value = entry.contentRect.height;
    }
  });
  observer.observe(element);
}
onBeforeUnmount(() => observer?.disconnect());

// 1, 2, 2.5 or 5 times a power of ten, at least the largest value
function niceMax(value: number) {
  if (value <= 0) return 1;
  const power = 10 ** Math.floor(Math.log10(value));
  const step = [1, 2, 2.5, 5, 10].find((f) => f * power >= value)!;
  return step * power;
}

const bottom = computed(() => height.value - PAD_BOTTOM);
const top = computed(() => niceMax(Math.max(0, ...props.values)));
const usable = computed(() => Math.max(40, bottom.value - PAD_TOP - 14));
// the labels are the x axis; values can briefly lag behind them (the new period's data is still loading), so positions
// come from the labels and only the values that fit are plotted
const slots = computed(() => props.labels.length || props.values.length);
const step = computed(() => (width.value - PAD_X * 2) / Math.max(1, slots.value - 1));
const xOf = (index: number) => PAD_X + index * step.value;
const yOf = (value: number) => PAD_TOP + 14 + (1 - value / top.value) * usable.value;

const coords = computed(() =>
  props.values.slice(0, slots.value).map((v, i) => [xOf(i), yOf(v)] as [number, number]),
);

const compact = new Intl.NumberFormat(undefined, { notation: "compact", maximumFractionDigits: 1 });
const ticks = computed(() =>
  [0, 0.5, 1].map((f) => ({ y: yOf(top.value * f), label: f && Math.max(...props.values) > 0 ? compact.format(top.value * f) : "" })),
);

// Catmull-Rom to cubic Bézier, as in the design; control points are clamped to the plot so a jump
// from zero does not swing the curve below the baseline
const line = computed(() => {
  const pts = coords.value;
  if (pts.length < 2) return "";
  const clamp = (y: number) => Math.min(bottom.value, Math.max(0, y));
  const d = [`M${pts[0][0]},${pts[0][1]}`];
  for (let i = 0; i < pts.length - 1; i++) {
    const [p0, p1, p2, p3] = [pts[i - 1] ?? pts[i], pts[i], pts[i + 1], pts[i + 2] ?? pts[i + 1]];
    const c1 = [p1[0] + (p2[0] - p0[0]) / 6, clamp(p1[1] + (p2[1] - p0[1]) / 6)];
    const c2 = [p2[0] - (p3[0] - p1[0]) / 6, clamp(p2[1] - (p3[1] - p1[1]) / 6)];
    d.push(`C${c1[0]},${c1[1]} ${c2[0]},${c2[1]} ${p2[0]},${p2[1]}`);
  }
  return d.join(" ");
});

const area = computed(() => {
  const pts = coords.value;
  if (pts.length < 2) return "";
  return `${line.value} L${pts[pts.length - 1][0]},${bottom.value} L${pts[0][0]},${bottom.value} Z`;
});

// a hovered index can outlive the data it pointed at (e.g. switching from 12 months to 8 quarters)
watch(
  () => props.values.length,
  () => (hover.value = null),
);

const selected = computed(() => {
  const point = hover.value === null ? undefined : coords.value[hover.value];
  return point ? { x: point[0], y: point[1] } : null;
});

// keep the tooltip inside the chart
const tipX = computed(() => Math.min(width.value - 70, Math.max(70, selected.value?.x ?? 0)));

function onMove(event: MouseEvent) {
  const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
  const index = Math.round((event.clientX - rect.left - PAD_X) / step.value);
  hover.value = coords.value.length ? Math.max(0, Math.min(coords.value.length - 1, index)) : null;
}
</script>

<style lang="scss">
.trend-chart {
  flex: 1;
  min-height: 0;
  position: relative;
  width: 100%;

  svg {
    display: block;
  }

  &__grid {
    stroke: #f0f0f0;
    stroke-dasharray: 4 4;
  }

  &__tick {
    fill: #a3a3a3;
    font-size: 11px;
  }

  &__label {
    fill: #9ca3af;
    font-size: 13px;

    &--active {
      fill: #0f3a57;
      font-weight: 600;
    }
  }

  &__tip {
    align-items: center;
    background: #fff;
    border: 1px solid #ececec;
    border-radius: 9px;
    box-shadow: 0 4px 12px rgba(23, 23, 23, 0.08);
    display: flex;
    flex-direction: column;
    gap: 2px;
    padding: 6px 12px;
    pointer-events: none;
    position: absolute;
    transform: translate(-50%, -100%);
    white-space: nowrap;

    strong {
      color: var(--neutrals-900);
      font-size: 15px;
      font-weight: 700;
    }

    .dash-delta {
      font-size: 12px;
    }
  }
}
</style>
