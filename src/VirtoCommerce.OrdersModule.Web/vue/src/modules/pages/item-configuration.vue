<template>
  <VcBlade
    :title="$t('ORDERS.WIDGETS.CONFIGURATION')"
    width="50%"
  >
    <VcContainer>
      <div class="orders-form tw-space-y-4">
        <VcCard :header="$t('ORDERS.PAGES.CONFIGURATION.PRODUCTS')">
          <VcDataTable
            :items="byType('Product')"
            :state-key="'ORDER_CONFIG_PRODUCTS'"
            @row-click="(e: { data: AnyOperation }) => openProduct(e.data.productId)"
          >
            <VcColumn
              id="imageUrl"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.IMAGE')"
              width="64px"
            >
              <template #body="{ data: row }">
                <VcImage
                  :src="assetUrl(row.imageUrl)"
                  size="s"
                  aspect="1x1"
                  bordered
                  background="contain"
                />
              </template>
            </VcColumn>
            <VcColumn
              id="name"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.NAME')"
            />
            <VcColumn
              id="quantity"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.QUANTITY')"
              type="number"
            />
          </VcDataTable>
        </VcCard>
        <VcCard :header="$t('ORDERS.PAGES.CONFIGURATION.TEXTS')">
          <VcDataTable
            :items="byType('Text')"
            :state-key="'ORDER_CONFIG_TEXTS'"
          >
            <VcColumn
              id="customText"
              :title="$t('ORDERS.PAGES.CONFIGURATION.TEXT')"
            />
            <VcColumn
              id="quantity"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.QUANTITY')"
              type="number"
            />
          </VcDataTable>
        </VcCard>
        <VcCard :header="$t('ORDERS.PAGES.CONFIGURATION.FILES')">
          <VcDataTable
            :items="files"
            :state-key="'ORDER_CONFIG_FILES'"
            @row-click="(e: { data: AnyOperation }) => openFile(e.data.url)"
          >
            <VcColumn
              id="name"
              :title="$t('ORDERS.PAGES.DETAILS.ITEMS.NAME')"
            />
            <VcColumn
              id="contentType"
              :title="$t('ORDERS.PAGES.CONFIGURATION.CONTENT_TYPE')"
            />
            <VcColumn
              id="size"
              :title="$t('ORDERS.PAGES.CONFIGURATION.SIZE')"
              type="number"
            />
          </VcDataTable>
        </VcCard>
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
// customerOrder-item-configuration (+ -products / -texts / -files): the configured product's sections
import { notification, useBlade } from "@vc-shell/framework";
import { VcBlade, VcCard, VcColumn, VcContainer, VcDataTable, VcImage } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { assetUrl, type AnyOperation } from "../utils/operations";

defineBlade({ name: "OrderItemConfiguration" });

const { t } = useI18n({ useScope: "global" });
const { callParent, openBlade } = useBlade();
const items = ref<AnyOperation[]>([]);
onMounted(async () => (items.value = (await callParent<AnyOperation>("getEntity"))?.configurationItems ?? []));

const byType = (type: string) => items.value.filter((x) => x.type === type);
const files = computed(() => byType("File").flatMap((x) => x.files ?? []));
// only http(s) or same-site links (the AngularJS href sanitizer refused javascript: and the like), without access to this window
const openFile = (url?: string) =>
  url && /^(https?:\/\/|\/(?!\/))/i.test(url.trim()) && window.open(url.trim(), "_blank", "noopener");

const openProduct = (productId?: string) =>
  productId &&
  openBlade({ name: "CatalogProductDetails", param: productId }).catch(() =>
    notification.error(t("ORDERS.ALERTS.PRODUCT_UNAVAILABLE"), { timeout: 5000 }),
  );
</script>
